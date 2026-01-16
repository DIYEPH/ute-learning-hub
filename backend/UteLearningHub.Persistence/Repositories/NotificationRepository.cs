using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class NotificationRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : Repository<Notification, Guid>(dbContext, dateTimeProvider), INotificationRepository
{
    public IQueryable<NotificationRecipient> GetNotificationRecipientsQueryable()
        => _dbContext.NotificationRecipients.AsQueryable();

    public IQueryable<NotificationRecipient> GetNotificationRecipientsWithNotificationQueryable()
        => _dbContext.NotificationRecipients.Include(nr => nr.Notification).AsQueryable();

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(nr => nr.Notification)
            .Where(nr => nr.RecipientId == userId
                && !nr.IsRead
                && !nr.Notification.IsDeleted
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow)
            .CountAsync(ct);
    }

    public async Task CreateNotificationRecipientsAsync(
        Guid notificationId, IEnumerable<Guid> recipientIds,
        DateTimeOffset receivedAt, CancellationToken ct = default)
    {
        var recipients = recipientIds.Select(id => new NotificationRecipient
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            RecipientId = id,
            IsSent = false,
            IsRead = false,
            ReceivedAt = receivedAt
        }).ToList();
        await _dbContext.NotificationRecipients.AddRangeAsync(recipients, ct);
    }

    public async Task<NotificationRecipient?> GetNotificationRecipientAsync(
        Guid notificationId, Guid userId,
        bool disableTracking = false, CancellationToken ct = default)
    {
        var query = _dbContext.NotificationRecipients
            .Where(nr => nr.NotificationId == notificationId && nr.RecipientId == userId);
        if (disableTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<List<NotificationRecipient>> GetUnreadNotificationRecipientsAsync(
        Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(nr => nr.Notification)
            .Where(nr => nr.RecipientId == userId
                && !nr.IsRead
                && !nr.Notification.IsDeleted
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow)
            .ToListAsync(ct);
    }

    public void UpdateRecipient(NotificationRecipient recipient)
        => _dbContext.NotificationRecipients.Update(recipient);
}
