using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Notification : BaseEntity<Guid>, IAggregateRoot, IAuditable
{
    public Guid ObjectId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string Link { get; set; } = default!;
    public bool IsGlobal { get; set; }
    public DateTimeOffset ExpiredAt { get; set; }
    public NotificationType NotificationType { get; set; } = NotificationType.System;
    public NotificationPriorityType NotificationPriorityType { get; set; } = NotificationPriorityType.Low;
    public ICollection<NotificationRecipient> Recipients { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

}
