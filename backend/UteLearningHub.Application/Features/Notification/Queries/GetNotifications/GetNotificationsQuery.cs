using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Notification.Queries.GetNotifications;

public record GetNotificationsQuery : GetNotificationsRequest, IRequest<PagedResponse<NotificationDto>>;