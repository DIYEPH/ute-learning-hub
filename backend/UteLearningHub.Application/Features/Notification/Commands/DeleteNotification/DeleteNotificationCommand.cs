using MediatR;

namespace UteLearningHub.Application.Features.Notification.Commands.DeleteNotification;

public record DeleteNotificationCommand : IRequest
{
    public Guid Id { get; init; }
}
