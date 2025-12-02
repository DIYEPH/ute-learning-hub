using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.LeaveConversation;

public record LeaveConversationCommand : IRequest
{
    public Guid ConversationId { get; init; }
}

