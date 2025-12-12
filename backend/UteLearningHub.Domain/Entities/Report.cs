using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Report : SoftDeletableEntity<Guid>, IAuditable, IAggregateRoot
{
    public Guid? DocumentFileId { get; set; }
    public Guid? CommentId { get; set; }
    public string Content { get; set; } = default!;
    public DocumentFile? DocumentFile { get; set; }
    public Comment? Comment { get; set; }

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.PendingReview;
    public Guid? ReviewedById { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}

