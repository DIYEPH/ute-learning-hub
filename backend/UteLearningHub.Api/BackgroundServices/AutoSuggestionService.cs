using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;
using System.Text.Json;

namespace UteLearningHub.Api.BackgroundServices;

// Service tự động gợi ý nhóm học - chạy mỗi 5 phút
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

        // Lấy subjects có tài liệu
        var subjects = await db.Subjects
            .Where(s => !s.IsDeleted && s.Documents.Any(d => !d.IsDeleted))
            .Select(s => new { s.Id, s.SubjectName })
            .ToListAsync(ct);
        if (subjects.Count == 0) return;

        // Lấy subjects đã có nhóm
        var subjectsWithConv = await (
            from c in db.Conversations
            where c.SubjectId != null
                && (c.ConversationStatus == ConversationStatus.Active || c.ConversationStatus == ConversationStatus.Proposed)
            select c.SubjectId!.Value
        ).Distinct().ToListAsync();

        var existingSubjects = subjectsWithConv.ToHashSet();

        // Lấy user vectors (users bật gợi ý + có vector)
        var userVectors = await (
            from v in db.ProfileVectors 
            from u in db.Users
            where v.UserId == u.Id
                && v.IsActive
                && v.EmbeddingJson != null
                && u.IsSuggest 
                && !u.IsDeleted 
                && u.MajorId != null
            select new { v.UserId, v.EmbeddingJson }
        ).ToListAsync(ct);

        if (userVectors.Count < ProposalSettings.MinMembersToActivate) return;

        // Parse vectors
        var allUserVectorData = new List<UserVectorData>();
        foreach (var item in userVectors)
        {
            var vec = JsonSerializer.Deserialize<float[]>(item.EmbeddingJson!);
            if (vec != null)
                allUserVectorData.Add(new UserVectorData(item.UserId, vec));
        }

        // Xử lý từng subject
        var now = DateTimeOffset.UtcNow;
        foreach (var subject in subjects)
        {
            if (existingSubjects.Contains(subject.Id)) continue;

            // Tạo vector cho subject
            var topicVector = await embeddingService.ConvVectorAsync(
                new ConvVectorRequest { Subject = subject.SubjectName }, ct);

            // Tìm users tương tự
            var similarUsers = await recommendService.GetSimilarUsersAsync(
                topicVector, allUserVectorData, topK: 50, minScore: 0.5f, ct);

            if (similarUsers.Users.Count < ProposalSettings.MinMembersToActivate) continue;

            // Lọc users hợp lệ
            var eligibleUserIds = await FilterEligibleUsersAsync(db,
                similarUsers.Users.Select(u => u.UserId).ToList(), ct);
            if (eligibleUserIds.Count < ProposalSettings.MinMembersToActivate) continue;

            // Tạo proposal
            await CreateProposalAsync(db, convRepo, notificationRepo,
                subject.Id, subject.SubjectName, eligibleUserIds, similarUsers.Users, now, ct);
        }
        await db.SaveChangesAsync(ct);
    }

    // Lọc users không trong cooldown
    private static async Task<List<Guid>> FilterEligibleUsersAsync(
        ApplicationDbContext db, List<Guid> userIds, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var cooldownThreshold = now.AddDays(-ProposalSettings.CooldownDaysAfterDecline);

        var usersInCooldown = await (
            from m in db.Set<ConversationMember>()
            where userIds.Contains(m.UserId)
                && m.InviteStatus == MemberInviteStatus.Declined
                && m.RespondedAt > cooldownThreshold
            select m.UserId
            )
            .Distinct()
            .ToListAsync(ct);

        var ineligibleSet = usersInCooldown.ToHashSet();
        return userIds.Where(id => !ineligibleSet.Contains(id)).ToList();
    }

    // Tạo proposal group
    private static async Task CreateProposalAsync(
        ApplicationDbContext db, IConversationRepository convRepo, INotificationRepository notificationRepo,
        Guid subjectId, string subjectName, List<Guid> eligibleUserIds,
        IReadOnlyList<SimilarUserItem> similarUsers, DateTimeOffset now, CancellationToken ct)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ConversationName = $"Nhóm: {subjectName}",
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
        foreach (var userId in eligibleUserIds)
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

        // Gửi thông báo
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ObjectId = conversation.Id,
            Title = "Gợi ý tạo nhóm học mới!",
            Content = $"Bạn được gợi ý tham gia nhóm học {subjectName} với {eligibleUserIds.Count - 1} người có cùng sở thích.",
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

