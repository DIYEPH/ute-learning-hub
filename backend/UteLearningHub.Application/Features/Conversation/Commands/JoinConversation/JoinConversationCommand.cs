using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.JoinConversation;

public record JoinConversationCommand : IRequest<ConversationDetailDto>
{
    public Guid ConversationId { get; init; }
}

