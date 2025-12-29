using MediatR;

namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public record MarkMessageAsReadCommand : IRequest
{
    public Guid ConversationId { get; init; }
    public Guid MessageId { get; init; }
}