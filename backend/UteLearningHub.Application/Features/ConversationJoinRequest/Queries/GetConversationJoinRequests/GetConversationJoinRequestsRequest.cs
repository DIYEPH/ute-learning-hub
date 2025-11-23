using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.ConversationJoinRequest.Queries.GetConversationJoinRequests;

public record GetConversationJoinRequestsRequest : PagedRequest
{
    public Guid? ConversationId { get; init; }
    public Guid? CreatedById { get; init; }
    public ReviewStatus? ReviewStatus { get; init; }
    public string? SearchTerm { get; init; }
}
