using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface INotificationRepository : IRepository<Notification, Guid>
{
    IQueryable<NotificationRecipient> GetNotificationRecipientsQueryable();
    IQueryable<NotificationRecipient> GetNotificationRecipientsWithNotificationQueryable();
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CreateNotificationRecipientsAsync(Guid notificationId, IEnumerable<Guid> recipientIds, DateTimeOffset receivedAt, CancellationToken cancellationToken = default);
    Task<NotificationRecipient?> GetNotificationRecipientAsync(Guid notificationId, Guid userId, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<List<NotificationRecipient>> GetUnreadNotificationRecipientsAsync(Guid userId, CancellationToken cancellationToken = default);
}
