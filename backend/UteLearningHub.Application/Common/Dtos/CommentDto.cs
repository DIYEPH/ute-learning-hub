using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record CommentDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public Guid? ParentId { get; init; }
    public string Content { get; init; } = default!;
    public string AuthorName { get; init; } = default!;
    public string? AuthorAvatarUrl { get; init; }
    public Guid CreatedById { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public int ReplyCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
