using MediatR;

namespace UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand : IRequest<Unit>;