using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

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
    public DocumentFileProgressDto? Progress { get; init; }
}
