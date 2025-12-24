using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetSuggestedUsers;

public record GetSuggestedUsersQuery : IRequest<GetSuggestedUsersResponse>
{
    public Guid ConversationId { get; init; }
    public int TopK { get; init; } = 10;
    public float MinScore { get; init; } = 0.3f;
}

public record GetSuggestedUsersResponse
{
    public List<SuggestedUserDto> Users { get; init; } = [];
    public int TotalProcessed { get; init; }
}

public record SuggestedUserDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public float Similarity { get; init; }
    public int Rank { get; init; }
}
