using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationById;

public record GetConversationByIdQuery : IRequest<ConversationDetailDto>
{
    public Guid Id { get; init; }
}
