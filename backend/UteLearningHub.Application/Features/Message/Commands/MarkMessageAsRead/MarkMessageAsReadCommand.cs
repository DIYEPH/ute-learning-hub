using MediatR;

namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public record MarkMessageAsReadCommand : MarkMessageAsReadRequest, IRequest<Unit>;