using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Subject : BaseEntity<Guid>, IAggregateRoot, IAuditable<Guid>, IReviewable<Guid>
{
    public Guid MajorId { get; set; }
    public string SubjectName { get; set; } = default!;
    public string SubjectCode { get; set; } = default!;
    public Major Major { get; set; } = default!;
    public ICollection<Conversation> Conversations { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public string ReviewNote { get; set; } = default!;
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;

}
