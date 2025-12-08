using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Subject : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable, IReviewable
{
    public string SubjectName { get; set; } = default!;
    public string SubjectCode { get; set; } = default!;
    public ICollection<SubjectMajor> SubjectMajors { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Approved;

}
