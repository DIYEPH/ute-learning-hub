using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Notification.Commands.CreateNotification;

public record CreateNotificationCommand : IRequest<NotificationDto>
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