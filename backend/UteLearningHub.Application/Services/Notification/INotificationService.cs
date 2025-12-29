using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Notification.Commands.CreateNotification;
using UteLearningHub.Application.Features.Notification.Commands.DeleteNotification;
using UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;
using UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;
using UteLearningHub.Application.Features.Notification.Commands.UpdateNotification;

namespace UteLearningHub.Application.Services.Notification;

public interface INotificationService
{
    Task<NotificationDto> CreateAsync(CreateNotificationCommand request, CancellationToken ct = default);
    Task<NotificationDto> UpdateAsync(UpdateNotificationCommand request, CancellationToken ct = default);
    Task DeleteAsync(DeleteNotificationCommand request, CancellationToken ct = default);
    Task MarkAsReadAsync(MarkAsReadCommand request, CancellationToken ct = default);
    Task MarkAllAsReadAsync(MarkAllAsReadCommand request, CancellationToken ct = default);
}
