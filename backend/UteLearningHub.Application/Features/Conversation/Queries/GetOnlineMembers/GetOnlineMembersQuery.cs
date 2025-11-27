using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetOnlineMembers;

public record GetOnlineMembersQuery(Guid ConversationId) : IRequest<GetOnlineMembersResponse>;
