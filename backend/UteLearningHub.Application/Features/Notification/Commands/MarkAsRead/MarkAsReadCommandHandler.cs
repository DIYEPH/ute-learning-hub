using MediatR;
using UteLearningHub.Application.Services.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;

public class MarkAsReadCommandHandler(INotificationService notificationService)
    : IRequestHandler<MarkAsReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        await notificationService.MarkAsReadAsync(request, cancellationToken);
        return Unit.Value;
    }
}
