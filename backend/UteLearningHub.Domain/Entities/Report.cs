using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Report : BaseEntity<Guid>, IAuditable, IReviewable
{
    public Guid? DocumentId { get; set; }
    public Guid? CommentId { get; set; }
    public string Content { get; set; } = default!;
    public Document Document { get; set; } = default!;
    public Comment Comment { get; set; } = default!;

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.PendingReview;
}
