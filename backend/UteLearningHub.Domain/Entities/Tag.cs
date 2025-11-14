using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Tag : BaseEntity<Guid>, IAggregateRoot, IAuditable, IReviewable
{
    public string TagName { get; set; } = default!;
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;
}
