using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;

public record DeleteConversationCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
