using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace UteLearningHub.Persistence.Repositories;

public class NotificationRepository : Repository<Notification, Guid>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) 
        : base(dbContext, dateTimeProvider)
    {
    }

    public IQueryable<NotificationRecipient> GetNotificationRecipientsQueryable()
    {
        return _dbContext.NotificationRecipients.AsQueryable();
    }

    public IQueryable<NotificationRecipient> GetNotificationRecipientsWithNotificationQueryable()
    {
        return _dbContext.NotificationRecipients
            .Include(nr => nr.Notification)
            .AsQueryable();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(nr => nr.Notification)
            .Where(nr => nr.RecipientId == userId 
                && !nr.IsDeleted 
                && !nr.IsRead
                && !nr.Notification.IsDeleted
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow)
            .CountAsync(cancellationToken);
    }

    public async Task CreateNotificationRecipientsAsync(
        Guid notificationId, 
        IEnumerable<Guid> recipientIds, 
        DateTimeOffset receivedAt, 
        CancellationToken cancellationToken = default)
    {
        var recipients = recipientIds.Select(recipientId => new NotificationRecipient
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            RecipientId = recipientId,
            IsSent = false,
            IsRead = false,
            ReceivedAt = receivedAt
        }).ToList();

        await _dbContext.NotificationRecipients.AddRangeAsync(recipients, cancellationToken);
    }

    public async Task<NotificationRecipient?> GetNotificationRecipientAsync(
        Guid notificationId, 
        Guid userId, 
        bool disableTracking = false, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.NotificationRecipients
            .Where(nr => nr.NotificationId == notificationId 
                && nr.RecipientId == userId 
                && !nr.IsDeleted);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<NotificationRecipient>> GetUnreadNotificationRecipientsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.NotificationRecipients
            .Include(nr => nr.Notification)
            .Where(nr => nr.RecipientId == userId 
                && !nr.IsDeleted 
                && !nr.IsRead
                && !nr.Notification.IsDeleted
                && nr.Notification.ExpiredAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
