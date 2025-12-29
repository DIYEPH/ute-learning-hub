using MediatR;
using UteLearningHub.Application.Services.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler(INotificationService notificationService)
    : IRequestHandler<DeleteNotificationCommand>
{
    public async Task Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        => await notificationService.DeleteAsync(request, cancellationToken);
}
