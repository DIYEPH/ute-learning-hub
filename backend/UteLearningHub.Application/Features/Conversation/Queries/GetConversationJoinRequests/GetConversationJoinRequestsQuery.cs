using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationJoinRequests;

public record GetConversationJoinRequestsQuery : GetConversationJoinRequestsRequest, IRequest<PagedResponse<ConversationJoinRequestDto>>;
