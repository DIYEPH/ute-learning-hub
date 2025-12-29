using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;

public class UpdateNotificationCommandHandler(INotificationService notificationService)
    : IRequestHandler<UpdateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(UpdateNotificationCommand request, CancellationToken cancellationToken)
        => await notificationService.UpdateAsync(request, cancellationToken);
}
