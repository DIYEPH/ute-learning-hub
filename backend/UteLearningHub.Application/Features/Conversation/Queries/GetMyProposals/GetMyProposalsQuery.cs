using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyProposals;

public record GetMyProposalsQuery : IRequest<GetMyProposalsResponse>;

public record GetMyProposalsResponse
{
    public IList<ProposalDto> Proposals { get; init; } = [];
}

public record ProposalDto
{
    public Guid ConversationId { get; init; }
    public string ConversationName { get; init; } = "";
    public string? SubjectName { get; init; }
    public IList<string> Tags { get; init; } = [];
    public string? AvatarUrl { get; init; }

    public int TotalMembers { get; init; }
    public int AcceptedCount { get; init; }
    public int PendingCount { get; init; }
    public int DeclinedCount { get; init; }

    public MemberInviteStatus MyStatus { get; init; }
    public float? MySimilarityScore { get; init; }

    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }

    public IList<ProposalMemberDto> Members { get; init; } = [];
}

public record ProposalMemberDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public MemberInviteStatus Status { get; init; }
    public float? SimilarityScore { get; init; }
    public DateTimeOffset? RespondedAt { get; init; }
}

