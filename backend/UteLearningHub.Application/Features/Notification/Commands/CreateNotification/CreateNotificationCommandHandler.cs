using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Notification;

namespace UteLearningHub.Application.Features.Notification.Commands.CreateNotification;

public class CreateNotificationCommandHandler(INotificationService notificationService)
    : IRequestHandler<CreateNotificationCommand, NotificationDto>
{
    public async Task<NotificationDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        => await notificationService.CreateAsync(request, cancellationToken);
}
