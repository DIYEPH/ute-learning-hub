using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Commands.ReviewConversationJoinRequest;

public record ReviewConversationJoinRequestRequest
{
    public Guid JoinRequestId { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public string? ReviewNote { get; init; }
}
