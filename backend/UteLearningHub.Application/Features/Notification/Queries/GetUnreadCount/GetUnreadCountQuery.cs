using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Notification.Queries.GetUnreadCount;

public record GetUnreadCountQuery : IRequest<UnreadCountDto>;
