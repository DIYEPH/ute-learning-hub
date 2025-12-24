using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record DocumentDetailDto
{
    public Guid Id { get; init; }
    public string DocumentName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public VisibilityStatus Visibility { get; init; }
    public SubjectDto? Subject { get; init; }
    public TypeDto Type { get; init; } = default!;
    public IList<TagDto> Tags { get; init; } = [];
    public IList<AuthorDto> Authors { get; init; } = [];
    public Guid? CoverFileId { get; init; }
    public IList<DocumentFileDto> Files { get; init; } = [];
    public int CommentCount { get; init; }
    public int UsefulCount { get; init; }
    public int NotUsefulCount { get; init; }
    public int TotalViewCount { get; init; }  
    public Guid CreatedById { get; init; }
    public string? CreatedByName { get; init; }
    public string? CreatedByAvatarUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record DocumentFileDto
{
    public Guid Id { get; init; }
    public Guid FileId { get; init; }
    public long FileSize { get; init; }
    public string MimeType { get; init; } = default!;
    public string? Title { get; init; }
    public int? Order { get; init; }
    public int? TotalPages { get; init; }
    public Guid? CoverFileId { get; init; }
    public ContentStatus Status { get; init; }
    
    // Review info
    public Guid? ReviewedById { get; init; }
    public DateTimeOffset? ReviewedAt { get; init; }
    public string? ReviewNote { get; init; }

    // Thống kê theo từng DocumentFile
    public int CommentCount { get; init; }
    public int UsefulCount { get; init; }
    public int NotUsefulCount { get; init; }
    public int ViewCount { get; init; }  

    // Progress tracking
    public DocumentProgressDto? Progress { get; init; }
}


