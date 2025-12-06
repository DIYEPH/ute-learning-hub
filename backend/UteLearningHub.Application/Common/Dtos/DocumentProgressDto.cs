namespace UteLearningHub.Application.Common.Dtos;

public record DocumentProgressDto
{
    public Guid DocumentFileId { get; init; }
    public int LastPage { get; init; }
    public int? TotalPages { get; init; }
    public DateTimeOffset? LastAccessedAt { get; init; }
}

