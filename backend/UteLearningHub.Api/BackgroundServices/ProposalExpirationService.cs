using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

// Service xử lý proposals hết hạn - chạy mỗi giờ
public class ProposalExpirationService(IServiceProvider sp, ILogger<ProposalExpirationService> log) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);
    private readonly TimeSpan _initialDelay = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await Task.Delay(_initialDelay, ct);
        while (!ct.IsCancellationRequested)
        {
            try { await ProcessExpiredProposalsAsync(ct); }
            catch (Exception ex) { log.LogError(ex, "Proposal expiration failed"); }
            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessExpiredProposalsAsync(CancellationToken ct)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var now = DateTimeOffset.UtcNow;

        // Tìm proposals hết hạn
        var expiredProposals = await db.Conversations
            .Include(c => c.Members)
            .Where(c => !c.IsDeleted
                && c.ConversationStatus == ConversationStatus.Proposed
                && c.ProposalExpiresAt.HasValue
                && c.ProposalExpiresAt.Value < now)
            .ToListAsync(ct);

        if (expiredProposals.Count == 0) return;

        foreach (var proposal in expiredProposals)
        {
            // Đánh dấu ended
            proposal.ConversationStatus = ConversationStatus.Ended;
            proposal.IsDeleted = true;

            // Gửi thông báo cho members đã accept
            var acceptedMembers = proposal.Members
                .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted)
                .Select(m => m.UserId)
                .ToList();

            if (acceptedMembers.Count > 0)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    Title = "Nhóm gợi ý đã hết hạn",
                    Content = $"Nhóm \"{proposal.ConversationName}\" không đủ người đồng ý nên không được tạo.",
                    Link = "",
                    IsGlobal = false,
                    NotificationType = NotificationType.System,
                    NotificationPriorityType = NotificationPriorityType.Normal,
                    CreatedAt = now
                };
                notificationRepo.Add(notification);
                await notificationRepo.CreateNotificationRecipientsAsync(notification.Id, acceptedMembers, now, ct);
            }
        }
        await db.SaveChangesAsync(ct);
    }
}