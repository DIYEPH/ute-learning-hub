using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class NotificationRecipient : SoftDeletableEntity<Guid>
{
    public Guid NotificationId { get; set; }
    public Guid RecipientId { get; set; }
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset ReadAt { get; set; }
    public Notification Notification { get; set; } = default!;
}
