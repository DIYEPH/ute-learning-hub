using MediatR;

namespace UteLearningHub.Application.Features.Message.Commands.DeleteMessage;

public record DeleteMessageCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
}