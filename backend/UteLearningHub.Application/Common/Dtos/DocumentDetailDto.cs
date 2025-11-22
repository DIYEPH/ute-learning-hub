using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record DocumentDetailDto
{
    public Guid Id { get; init; }
    public string DocumentName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string AuthorName { get; init; } = default!;
    public string DescriptionAuthor { get; init; } = default!;
    public bool IsDownload { get; init; }
    public VisibilityStatus Visibility { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public SubjectDto Subject { get; init; } = default!;
    public TypeDto Type { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public IList<DocumentFileDto> Files { get; init; } = [];
    public int CommentCount { get; init; }
    public Guid CreatedById { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record DocumentFileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = default!;
    public string FileUrl { get; init; } = default!;
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
}

