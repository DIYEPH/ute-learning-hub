using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ReportDto
{
    public Guid Id { get; init; }
    public Guid? DocumentId { get; init; }
    public Guid? CommentId { get; init; }
    public string Content { get; init; } = default!;
    public string ReporterName { get; init; } = default!;
    public string? ReporterAvatarUrl { get; init; }
    public Guid CreatedById { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
