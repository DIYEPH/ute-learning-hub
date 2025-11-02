using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Major : BaseEntity<Guid>, IAggregateRoot, IAuditable<Guid>, IReviewable<Guid>
{
    public Guid FacultyId { get; set; }
    public string MajorName { get; set; } = default!;
    public string MajorCode { get; set; } = default!;
    public Faculty Faculty { get; set; } = default!;
    public ICollection<Subject> Subjects { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public string ReviewNote { get; set; } = default!;
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.PendingReview;
}
