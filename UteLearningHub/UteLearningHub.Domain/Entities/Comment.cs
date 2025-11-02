using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Comment : BaseEntity<Guid>, IAuditable<Guid>, IReviewable<Guid>
{
    public Guid ParentId { get; set; }
    public Guid DocumentId { get; set; }
    public string Content { get; set; } = default!;
    public Document Document { get; set; } = default!;
    public ICollection<Report> Reports { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public string ReviewNote { get; set; } = default!;
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
}
