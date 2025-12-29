using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public record UpdateConversationCommand : UpdateConversationCommandRequest, IRequest<ConversationDetailDto>
{
    public Guid Id { get; init; }
}
