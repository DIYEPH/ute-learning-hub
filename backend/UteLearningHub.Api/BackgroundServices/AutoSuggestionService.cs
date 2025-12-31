using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

/// <summary>
/// Background service xử lý SuggestionPool - cluster users và tạo proposals
/// Chạy mỗi 5 phút để xử lý users 
/// </summary>
public class AutoSuggestionService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<AutoSuggestionService> _log;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); 
    private readonly TimeSpan _initialDelay = TimeSpan.FromSeconds(30); 

    public AutoSuggestionService(IServiceProvider sp, ILogger<AutoSuggestionService> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("AutoSuggestionService started");
        await Task.Delay(_initialDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessSuggestionsAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Auto suggestion processing failed");
            }

            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessSuggestionsAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var recommendService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();
        var profileStore = scope.ServiceProvider.GetRequiredService<IProfileVectorStore>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var convRepo = scope.ServiceProvider.GetRequiredService<IConversationRepository>();

        _log.LogInformation("Processing auto suggestions...");

        // 1. Lấy TẤT CẢ users eligible (IsSuggest=true, có MajorId, có ProfileVector)
        var usersWithVectors = await db.Users
            .Where(u => u.IsSuggest && !u.IsDeleted && u.MajorId != null)
            .Join(
                db.ProfileVectors.Where(v => v.IsActive),
                u => u.Id,
                v => v.UserId,
                (u, v) => new { u.Id, v.EmbeddingJson }
            )
            .ToListAsync(ct);

        if (usersWithVectors.Count == 0)
        {
            _log.LogDebug("No eligible users with vectors found");
            return;
        }

        _log.LogInformation("Found {Count} eligible users with vectors", usersWithVectors.Count);

        // 2. Filter theo các điều kiện chi tiết (số nhóm, pending proposals, cooldown)
        var proposalPool = new List<(Guid UserId, float[] Vector)>();

        foreach (var user in usersWithVectors)
        {
            try
            {
                // Kiểm tra điều kiện (số nhóm, pending proposals, cooldown)
                if (!await IsUserEligibleForProposalAsync(db, user.Id, ct))
                    continue;

                // Parse vector
                var vector = System.Text.Json.JsonSerializer.Deserialize<float[]>(user.EmbeddingJson);
                if (vector == null || vector.Length == 0)
                {
                    _log.LogWarning("User {UserId} has invalid vector", user.Id);
                    continue;
                }

                proposalPool.Add((user.Id, vector));
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to process user {UserId}", user.Id);
            }
        }

        _log.LogInformation("After eligibility check: {Count} users ready for clustering", proposalPool.Count);

        // 3. Nếu đủ người thì tạo proposals
        if (proposalPool.Count >= ProposalSettings.MinMembersToActivate)
        {
            await GenerateProposalsFromPoolAsync(db, convRepo, notificationRepo, recommendService, proposalPool, ct);
        }
        else
        {
            _log.LogDebug("Not enough eligible users ({Count} < {Min})", 
                proposalPool.Count, ProposalSettings.MinMembersToActivate);
        }

        await db.SaveChangesAsync(ct);
        _log.LogInformation("Auto suggestion processing completed");
    }

    private async Task<float[]?> GetUserVectorAsync(IProfileVectorStore store, Guid userId, CancellationToken ct)
    {
        var vector = await store.Query()
            .Where(v => v.UserId == userId && v.IsActive)
            .OrderByDescending(v => v.CalculatedAt)
            .FirstOrDefaultAsync(ct);

        if (vector == null || string.IsNullOrEmpty(vector.EmbeddingJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<float[]>(vector.EmbeddingJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Kiểm tra user có đủ điều kiện nhận proposal không
    /// </summary>
    private async Task<bool> IsUserEligibleForProposalAsync(ApplicationDbContext db, Guid userId, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var cooldownThreshold = now.AddDays(-ProposalSettings.CooldownDaysAfterDecline);

        // Check số nhóm đang tham gia
        var activeConvCount = await db.Set<ConversationMember>()
            .CountAsync(m =>
                m.UserId == userId &&
                !m.IsDeleted &&
                m.InviteStatus == MemberInviteStatus.Joined &&
                !m.Conversation.IsDeleted &&
                m.Conversation.ConversationStatus == ConversationStatus.Active, ct);

        if (activeConvCount >= ProposalSettings.MaxActiveConversations)
            return false;

        // Check số pending proposals
        var pendingProposalCount = await db.Set<ConversationMember>()
            .CountAsync(m =>
                m.UserId == userId &&
                !m.IsDeleted &&
                m.InviteStatus == MemberInviteStatus.Pending &&
                !m.Conversation.IsDeleted &&
                m.Conversation.ConversationStatus == ConversationStatus.Proposed, ct);

        if (pendingProposalCount >= ProposalSettings.MaxPendingProposals)
            return false;

        // Check cooldown sau khi decline
        var lastDecline = await db.Set<ConversationMember>()
            .Where(m => m.UserId == userId && m.InviteStatus == MemberInviteStatus.Declined)
            .OrderByDescending(m => m.RespondedAt)
            .Select(m => m.RespondedAt)
            .FirstOrDefaultAsync(ct);

        if (lastDecline.HasValue && lastDecline.Value > cooldownThreshold)
            return false;

        return true;
    }

    private async Task<List<Guid>> GenerateProposalsFromPoolAsync(
        ApplicationDbContext db,
        IConversationRepository convRepo,
        INotificationRepository notificationRepo,
        IRecommendationService recommendService,
        List<(Guid UserId, float[] Vector)> pool,
        CancellationToken ct)
    {
        var processedUserIds = new List<Guid>();
        _log.LogInformation("Generating proposals from pool of {Count} users", pool.Count);

        // Chuẩn bị data cho clustering
        var userVectorData = pool.Select(p => new UserVectorData(p.UserId, p.Vector)).ToList();

        // Gọi AI để cluster
        var clusterResult = await recommendService.ClusterSimilarUsersAsync(
            userVectorData,
            ProposalSettings.MinMembersToActivate,
            ct);

        _log.LogInformation("Created {Count} clusters", clusterResult.Clusters.Count);

        var now = DateTimeOffset.UtcNow;

        foreach (var cluster in clusterResult.Clusters)
        {
            if (cluster.Members.Count < ProposalSettings.MinMembersToActivate)
                continue;

            try
            {
                // Tìm common interests để đặt tên nhóm thông minh
                var memberIds = cluster.Members.Select(m => m.UserId).ToList();
                var (commonMajorName, commonTags) = await GetCommonInterestsAsync(db, memberIds, ct);

                // Đặt tên nhóm dựa trên common interests
                string conversationName;
                if (!string.IsNullOrEmpty(commonMajorName))
                {
                    conversationName = $"Nhóm học tập - {commonMajorName}";
                }
                else if (commonTags.Any())
                {
                    conversationName = $"Nhóm {string.Join(", ", commonTags.Take(2))}";
                }
                else
                {
                    conversationName = $"Nhóm học gợi ý #{DateTime.UtcNow:yyyyMMddHHmm}";
                }

                // Tạo conversation proposal
                var conversation = new Domain.Entities.Conversation
                {
                    Id = Guid.NewGuid(),
                    ConversationName = conversationName,
                    ConversationType = ConversitionType.Group,
                    ConversationStatus = ConversationStatus.Proposed,
                    IsSuggestedByAI = true,
                    Visibility = ConversationVisibility.Private,
                    ProposalExpiresAt = now.AddDays(ProposalSettings.ProposalExpirationDays),
                    CreatedById = cluster.Members.First().UserId,
                    CreatedAt = now
                };

                convRepo.Add(conversation);

                // Thêm members với status Pending
                foreach (var member in cluster.Members)
                {
                    var convMember = new ConversationMember
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = conversation.Id,
                        UserId = member.UserId,
                        ConversationMemberRoleType = ConversationMemberRoleType.Member,
                        InviteStatus = MemberInviteStatus.Pending,
                        SimilarityScore = member.SimilarityToCentroid,
                        CreatedAt = now
                    };

                    db.Set<ConversationMember>().Add(convMember);
                }

                await db.SaveChangesAsync(ct);

                // Gửi notification cho tất cả members
                foreach (var member in cluster.Members)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        ObjectId = conversation.Id,
                        Title = "AI gợi ý tạo nhóm học mới!",
                        Content = $"Bạn được mời tham gia nhóm học với {cluster.Members.Count - 1} người có cùng sở thích. Xem và phản hồi ngay!",
                        Link = $"/proposals/{conversation.Id}",
                        IsGlobal = false,
                        NotificationType = NotificationType.Conversation,
                        NotificationPriorityType = NotificationPriorityType.Hight,
                        ExpiredAt = now.AddDays(ProposalSettings.ProposalExpirationDays),
                        CreatedAt = now,
                        CreatedById = cluster.Members.First().UserId
                    };

                    notificationRepo.Add(notification);
                    await notificationRepo.CreateNotificationRecipientsAsync(
                        notification.Id, [member.UserId], now, ct);
                }

                _log.LogInformation("Created proposal {ConvId} with {Count} members",
                    conversation.Id, cluster.Members.Count);
                
                // Track processed users
                processedUserIds.AddRange(cluster.Members.Select(m => m.UserId));
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to create proposal for cluster");
            }
        }
        
        return processedUserIds;
    }

    /// <summary>
    /// Tìm common interests (Major và Tags) của các users trong cluster
    /// </summary>
    private async Task<(string? CommonMajorName, List<string> CommonTags)> GetCommonInterestsAsync(
        ApplicationDbContext db,
        List<Guid> memberIds,
        CancellationToken ct)
    {
        // 1. Tìm Major chung (nếu có)
        var userMajors = await db.Users
            .Where(u => memberIds.Contains(u.Id) && u.MajorId != null)
            .Select(u => new { u.MajorId, u.Major!.MajorName })
            .ToListAsync(ct);

        string? commonMajorName = null;
        if (userMajors.Any())
        {
            // Tìm major xuất hiện nhiều nhất
            var majorGroup = userMajors
                .GroupBy(x => x.MajorId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            // Nếu > 50% users có cùng major → dùng làm tên
            if (majorGroup != null && majorGroup.Count() > memberIds.Count / 2)
            {
                commonMajorName = majorGroup.First().MajorName;
            }
        }

        // 2. Tìm Tags chung (từ behavior hoặc profile)
        var commonTags = new List<string>();
        
        // Lấy tags từ user profiles
        var userTags = await db.Users
            .Where(u => memberIds.Contains(u.Id))
            .SelectMany(u => u.Tags.Select(t => t.TagName))
            .GroupBy(t => t)
            .Where(g => g.Count() >= 2) // Ít nhất 2 users có tag này
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(3)
            .ToListAsync(ct);

        commonTags.AddRange(userTags);

        return (commonMajorName, commonTags);
    }
}

