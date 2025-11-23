using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ConversationJoinRequestDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public string ConversationName { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string RequesterName { get; init; } = default!;
    public string? RequesterAvatarUrl { get; init; }
    public Guid CreatedById { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public string? ReviewNote { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
