using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Subject : BaseEntity<Guid>, IAggregateRoot, IAuditable, IReviewable
{
    public Guid? MajorId { get; set; }
    public string SubjectName { get; set; } = default!;
    public string SubjectCode { get; set; } = default!;
    public Major? Major { get; set; } 
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;

}
