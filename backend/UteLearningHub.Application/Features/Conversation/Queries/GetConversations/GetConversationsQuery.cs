using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversations;

public record GetConversationsQuery : GetConversationsRequest, IRequest<PagedResponse<ConversationDto>>;
