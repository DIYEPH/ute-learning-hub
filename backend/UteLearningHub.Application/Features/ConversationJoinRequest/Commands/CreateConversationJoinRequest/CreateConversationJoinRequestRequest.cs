namespace UteLearningHub.Application.Features.ConversationJoinRequest.Commands.CreateConversationJoinRequest;

public record CreateConversationJoinRequestRequest
{
    public Guid ConversationId { get; init; }
    public string Content { get; init; } = default!;
}