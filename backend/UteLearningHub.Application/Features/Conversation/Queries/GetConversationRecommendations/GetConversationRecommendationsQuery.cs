using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetConversationRecommendations;

public record GetConversationRecommendationsQuery : IRequest<GetConversationRecommendationsResponse>
{
    public int? TopK { get; init; }
    public float? MinSimilarity { get; init; }
}

public record GetConversationRecommendationsResponse
{
    public IReadOnlyList<ConversationRecommendationDto> Recommendations { get; init; } = Array.Empty<ConversationRecommendationDto>();
    public int TotalProcessed { get; init; }
    public double ProcessingTimeMs { get; init; }
}

public record ConversationRecommendationDto
{
    public Guid ConversationId { get; init; }
    public string ConversationName { get; init; } = default!;
    public float Similarity { get; init; }
    public int Rank { get; init; }
    public SubjectDto? Subject { get; init; }
    public IList<TagDto> Tags { get; init; } = [];
    public string? AvatarUrl { get; init; }
    public int MemberCount { get; init; }
    public bool? IsCurrentUserMember { get; init; }
    public bool? HasPendingJoinRequest { get; init; }
}


