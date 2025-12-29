using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record DocumentDto
{
    public Guid Id { get; init; }
    public string DocumentName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public VisibilityStatus Visibility { get; init; }
    public SubjectDto? Subject { get; init; }
    public TypeDto Type { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public IList<AuthorDetailDto> Authors { get; init; } = [];
    public Guid? ThumbnailFileId { get; init; }
    public int FileCount { get; init; }
    public int CommentCount { get; init; }
    public int UsefulCount { get; init; }
    public int NotUsefulCount { get; init; }
    public int TotalViewCount { get; init; }
    public Guid CreatedById { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
