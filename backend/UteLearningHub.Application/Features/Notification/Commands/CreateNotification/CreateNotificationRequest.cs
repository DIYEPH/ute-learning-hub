using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Notification.Commands.CreateNotification;

public record CreateNotificationRequest
{
    public Guid ObjectId { get; init; }
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string Link { get; init; } = default!;
    public bool IsGlobal { get; init; }
    public DateTimeOffset ExpiredAt { get; init; }
    public NotificationType NotificationType { get; init; } = NotificationType.System;
    public NotificationPriorityType NotificationPriorityType { get; init; } = NotificationPriorityType.Normal;
    public IList<Guid>? RecipientIds { get; init; }
}