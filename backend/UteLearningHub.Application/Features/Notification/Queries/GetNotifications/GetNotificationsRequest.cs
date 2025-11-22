using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Notification.Queries.GetNotifications;

public record GetNotificationsRequest : PagedRequest
{
    public bool? IsRead { get; init; }
    public NotificationType? NotificationType { get; init; }
    public NotificationPriorityType? NotificationPriorityType { get; init; }
}
