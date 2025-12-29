using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;

public record DeleteConversationCommand : IRequest
{
    public Guid Id { get; init; }
}
