using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ConversationDetailDto
{
    public Guid Id { get; init; }
    public string ConversationName { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public ConversitionType ConversationType { get; init; }
    public ConversationVisibility Visibility { get; init; }
    public ConversationStatus ConversationStatus { get; init; }
    public bool IsSuggestedByAI { get; init; }
    public bool IsAllowMemberPin { get; init; }
    public SubjectDto? Subject { get; init; }
    public string? AvatarUrl { get; init; }
    public IList<ConversationMemberDto> Members { get; init; } = [];
    public int MessageCount { get; init; }
    public Guid? LastMessageId { get; init; }
    public Guid? CreatedById { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}