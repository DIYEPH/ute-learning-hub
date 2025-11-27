namespace UteLearningHub.Application.Features.Conversation.Queries.GetOnlineMembers;

public record GetOnlineMembersResponse
{
    public Guid ConversationId { get; init; }
    public IList<Guid> OnlineUserIds { get; init; } = [];
}
