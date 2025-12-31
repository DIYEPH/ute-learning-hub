using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence;

namespace UteLearningHub.Api.BackgroundServices;

/// <summary>
/// Background service để xử lý proposals hết hạn
/// </summary>
public class ProposalExpirationService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<ProposalExpirationService> _log;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Kiểm tra mỗi giờ
    private readonly TimeSpan _initialDelay = TimeSpan.FromMinutes(15);

    public ProposalExpirationService(IServiceProvider sp, ILogger<ProposalExpirationService> log)
    {
        _sp = sp;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("ProposalExpirationService started");
        await Task.Delay(_initialDelay, ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredProposalsAsync(ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Proposal expiration processing failed");
            }

            await Task.Delay(_interval, ct);
        }
    }

    private async Task ProcessExpiredProposalsAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        var now = DateTimeOffset.UtcNow;

        // Tìm proposals đã hết hạn
        var expiredProposals = await db.Conversations
            .Include(c => c.Members)
            .Where(c => !c.IsDeleted
                && c.ConversationStatus == ConversationStatus.Proposed
                && c.ProposalExpiresAt.HasValue
                && c.ProposalExpiresAt.Value < now)
            .ToListAsync(ct);

        _log.LogInformation("Found {Count} expired proposals", expiredProposals.Count);

        foreach (var proposal in expiredProposals)
        {
            try
            {
                // Đánh dấu conversation là Ended
                proposal.ConversationStatus = ConversationStatus.Ended;
                proposal.IsDeleted = true;

                // Gửi notification cho những người đã Accept
                var acceptedMembers = proposal.Members
                    .Where(m => !m.IsDeleted && m.InviteStatus == MemberInviteStatus.Accepted)
                    .ToList();

                foreach (var member in acceptedMembers)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        Title = "⏰ Nhóm gợi ý đã hết hạn",
                        Content = $"Rất tiếc, nhóm \"{proposal.ConversationName}\" không đủ người đồng ý nên không được tạo.",
                        Link = "",
                        IsGlobal = false,
                        NotificationType = NotificationType.System,
                        NotificationPriorityType = NotificationPriorityType.Normal,
                        CreatedAt = now
                    };

                    notificationRepo.Add(notification);
                    await notificationRepo.CreateNotificationRecipientsAsync(
                        notification.Id, [member.UserId], now, ct);
                }

                _log.LogInformation("Expired proposal {Id}, notified {Count} accepted members",
                    proposal.Id, acceptedMembers.Count);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Failed to process expired proposal {Id}", proposal.Id);
            }
        }

        if (expiredProposals.Any())
        {
            await db.SaveChangesAsync(ct);
        }
    }
}

