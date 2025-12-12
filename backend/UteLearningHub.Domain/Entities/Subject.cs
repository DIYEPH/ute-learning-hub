using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Subject : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public string SubjectName { get; set; } = default!;
    public string SubjectCode { get; set; } = default!;
    public ICollection<SubjectMajor> SubjectMajors { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Approved;
}

