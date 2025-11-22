using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Notification.Commands.CreateNotification;

public record CreateNotificationCommand : CreateNotificationRequest, IRequest<NotificationDto>;