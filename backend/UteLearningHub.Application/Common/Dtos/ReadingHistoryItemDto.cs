namespace UteLearningHub.Application.Common.Dtos;

public record ReadingHistoryItemDto
{
    public Guid DocumentId { get; init; }
    public string DocumentName { get; init; } = string.Empty;
    public Guid? DocumentFileId { get; init; }
    public string? FileTitle { get; init; }
    public int LastPage { get; init; }
    public int? TotalPages { get; init; }
    public DateTimeOffset? LastAccessedAt { get; init; }
    public Guid? CoverFileId { get; init; }
    public string? SubjectName { get; init; }
}
