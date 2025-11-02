using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Faculty : BaseEntity<Guid>, IAggregateRoot, IAuditable<Guid>, IReviewable<Guid>
{
    public string FacultyName { get; set; } = default!;
    public string FacultyCode { get; set; } = default!;
    public ICollection<Major> Majors { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public string ReviewNote { get; set; } = default!;
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;
}
