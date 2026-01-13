using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

/// Chạy mỗi 5 phút để phát hiện subjects có nhiều users quan tâm và tạo proposal.
public class AutoSuggestionService(IServiceProvider sp, ILogger<AutoSuggestionService> log) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(_initialDelay, ct);
        while (!ct.IsCancellationRequested)
        {
            try { await ProcessAsync(ct); }
            catch (Exception ex) { log.LogError(ex, "AutoSuggestion failed"); }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();
        var recommendService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
        var convRepo = scope.ServiceProvider.GetRequiredService<IConversationRepository>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        // 1. Lấy tất cả Subjects có documents (có hoạt động)
        var subjects = await db.Subjects
            .Where(s => !s.IsDeleted && s.Documents.Any(d => !d.IsDeleted))
            .Select(s => new { s.Id, s.SubjectName })
            .ToListAsync(ct);
        if (subjects.Count == 0) return;

        // 2. Lấy SubjectIds đã có Conversation Active
        var subjectsActive = await db.Conversations
            .Where(c => !c.IsDeleted && c.SubjectId != null && c.ConversationStatus == ConversationStatus.Active)
            .Select(c => c.SubjectId!.Value)
            .Distinct()
            .ToListAsync(ct);
        var subjectsActiveSet = subjectsActive.ToHashSet();

        // 3. Lấy tất cả user vectors (users eligible)
        var userVectors = await db.ProfileVectors
            .Where(v => v.IsActive && !string.IsNullOrEmpty(v.EmbeddingJson))
            .Join(db.Users.Where(u => u.IsSuggest && !u.IsDeleted && u.MajorId != null),
                v => v.UserId, u => u.Id, (v, u) => new { v.UserId, v.EmbeddingJson })
            .ToListAsync(ct);
        if (userVectors.Count < ProposalSettings.MinMembersToActivate) return;

        var allUserVectorData = userVectors
            .Select(x => {
                var vec = System.Text.Json.JsonSerializer.Deserialize<float[]>(x.EmbeddingJson);
                return vec != null ? new UserVectorData(x.UserId, vec) : null;
            })
            .Where(x => x != null)
            .Cast<UserVectorData>()
            .ToList();

        // 4. Với mỗi Subject chưa có group, dùng AI tìm users quan tâm
        var now = DateTimeOffset.UtcNow;
        foreach (var subject in subjects)
        {
            if (subjectsActiveSet.Contains(subject.Id)) continue;

            // Tạo topic vector cho subject
            var topicVector = await embeddingService.ConvVectorAsync(
                new ConvVectorRequest { Subject = subject.SubjectName }, ct);

            // Tìm users có similarity với topic
            var similarUsers = await recommendService.GetSimilarUsersAsync(
                topicVector, allUserVectorData, topK: 50, minScore: 0.5f, ct);

            if (similarUsers.Users.Count < ProposalSettings.MinMembersToActivate) continue;

            // Kiểm tra user eligibility (chưa có quá nhiều pending/active groups)
            var eligibleUserIds = await FilterEligibleUsersAsync(db, 
                similarUsers.Users.Select(u => u.UserId).ToList(), ct);
            if (eligibleUserIds.Count < ProposalSettings.MinMembersToActivate) continue;

            // Tạo proposal group
            await CreateProposalAsync(db, convRepo, notificationRepo, 
                subject.Id, subject.SubjectName, eligibleUserIds, similarUsers.Users, now, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<List<Guid>> FilterEligibleUsersAsync(
        ApplicationDbContext db, List<Guid> userIds, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var cooldownThreshold = now.AddDays(-ProposalSettings.CooldownDaysAfterDecline);

        // Lấy users không đủ điều kiện (quá nhiều active/pending groups hoặc trong cooldown)
        var ineligibleUsers = await db.Set<ConversationMember>()
            .Where(m => userIds.Contains(m.UserId) && !m.IsDeleted && !m.Conversation.IsDeleted)
            .GroupBy(m => m.UserId)
            .Where(g =>
                g.Count(m => m.InviteStatus == MemberInviteStatus.Joined && 
                    m.Conversation.ConversationStatus == ConversationStatus.Active) >= ProposalSettings.MaxActiveConversations ||
                g.Count(m => (m.InviteStatus == MemberInviteStatus.Pending || m.InviteStatus == MemberInviteStatus.Accepted) && 
                    m.Conversation.ConversationStatus == ConversationStatus.Proposed) >= ProposalSettings.MaxPendingProposals)
            .Select(g => g.Key)
            .ToListAsync(ct);

        // Lấy users trong cooldown
        var usersInCooldown = await db.Set<ConversationMember>()
            .Where(m => userIds.Contains(m.UserId) && m.InviteStatus == MemberInviteStatus.Declined && 
                m.RespondedAt > cooldownThreshold)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        var ineligibleSet = ineligibleUsers.Concat(usersInCooldown).ToHashSet();
        return userIds.Where(id => !ineligibleSet.Contains(id)).ToList();
    }

    private static async Task CreateProposalAsync(
        ApplicationDbContext db, IConversationRepository convRepo, INotificationRepository notificationRepo,
        Guid subjectId, string subjectName, List<Guid> eligibleUserIds,
        IReadOnlyList<SimilarUserItem> similarUsers, DateTimeOffset now, CancellationToken ct)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ConversationName = $"Nhóm học - {subjectName}",
            SubjectId = subjectId,
            ConversationType = ConversitionType.Group,
            ConversationStatus = ConversationStatus.Proposed,
            Visibility = ConversationVisibility.Private,
            IsSuggestedByAI = true,
            ProposalExpiresAt = now.AddDays(ProposalSettings.ProposalExpirationDays),
            CreatedById = eligibleUserIds.First(),
            CreatedAt = now
        };
        convRepo.Add(conversation);

        // Thêm members
        var similarityMap = similarUsers.ToDictionary(u => u.UserId, u => u.Similarity);
        foreach (var userId in eligibleUserIds.Take(ProposalSettings.MaxMembersPerProposal))
        {
            db.Set<ConversationMember>().Add(new ConversationMember
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                UserId = userId,
                ConversationMemberRoleType = ConversationMemberRoleType.Member,
                InviteStatus = MemberInviteStatus.Pending,
                SimilarityScore = similarityMap.GetValueOrDefault(userId, 0),
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(ct);

        // Gửi notification
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ObjectId = conversation.Id,
            Title = "AI gợi ý tạo nhóm học mới!",
            Content = $"Bạn được mời tham gia nhóm học {subjectName} với {eligibleUserIds.Count - 1} người có cùng sở thích.",
            Link = "/conversations",
            IsGlobal = false,
            NotificationType = NotificationType.Conversation,
            NotificationPriorityType = NotificationPriorityType.Hight,
            ExpiredAt = now.AddDays(ProposalSettings.ProposalExpirationDays),
            CreatedAt = now,
            CreatedById = eligibleUserIds.First()
        };
        notificationRepo.Add(notification);
        await notificationRepo.CreateNotificationRecipientsAsync(notification.Id, eligibleUserIds, now, ct);
    }
}