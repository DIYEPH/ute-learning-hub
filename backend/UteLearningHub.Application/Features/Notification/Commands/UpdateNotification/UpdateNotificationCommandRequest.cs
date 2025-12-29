using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;

public record UpdateNotificationCommandRequest
{
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string? Link { get; init; }
    public DateTimeOffset ExpiredAt { get; init; }
    public NotificationType NotificationType { get; init; }
    public NotificationPriorityType NotificationPriorityType { get; init; }
}
