using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversationJoinRequest;

public record CreateConversationJoinRequestCommand : IRequest<ConversationJoinRequestDto>
{
    public Guid ConversationId { get; init; }
    public string Content { get; init; } = default!;
}
