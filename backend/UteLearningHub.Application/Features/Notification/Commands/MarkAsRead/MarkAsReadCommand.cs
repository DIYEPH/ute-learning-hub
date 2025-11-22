using MediatR;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;

public record MarkAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; init; }
}
