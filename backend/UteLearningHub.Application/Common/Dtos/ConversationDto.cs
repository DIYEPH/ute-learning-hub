using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ConversationDto
{
    public Guid Id { get; init; }
    public string ConversationName { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public ConversitionType ConversationType { get; init; }
    public ConversationStatus ConversationStatus { get; init; }
    public bool IsSuggestedByAI { get; init; }
    public bool IsAllowMemberPin { get; init; }
    public SubjectDto? Subject { get; init; }
    public string CreatorName { get; init; } = default!;
    public string? CreatorAvatarUrl { get; init; }
    public int MemberCount { get; init; }
    public int MessageCount { get; init; }
    public Guid? LastMessageId { get; init; }
    public Guid CreatedById { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record ConversationDetailDto
{
    public Guid Id { get; init; }
    public string ConversationName { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public ConversitionType ConversationType { get; init; }
    public ConversationStatus ConversationStatus { get; init; }
    public bool IsSuggestedByAI { get; init; }
    public bool IsAllowMemberPin { get; init; }
    public SubjectDto? Subject { get; init; }
    public string CreatorName { get; init; } = default!;
    public string? CreatorAvatarUrl { get; init; }
    public IList<ConversationMemberDto> Members { get; init; } = [];
    public int MessageCount { get; init; }
    public Guid? LastMessageId { get; init; }
    public Guid CreatedById { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record ConversationMemberDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = default!;
    public string? UserAvatarUrl { get; init; }
    public ConversationMemberRoleType RoleType { get; init; }
    public bool IsMuted { get; init; }
    public DateTimeOffset JoinedAt { get; init; }
}
