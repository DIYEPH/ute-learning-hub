using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;

public record UpdateNotificationCommand : UpdateNotificationCommandRequest, IRequest<NotificationDto>
{
    public Guid Id { get; init; }
}
