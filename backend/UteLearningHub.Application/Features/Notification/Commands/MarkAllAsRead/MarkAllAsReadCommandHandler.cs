using MediatR;
using UteLearningHub.Application.Services.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler(INotificationService notificationService)
    : IRequestHandler<MarkAllAsReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        await notificationService.MarkAllAsReadAsync(request, cancellationToken);
        return Unit.Value;
    }
}
