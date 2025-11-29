using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Major : BaseEntity<Guid>, IAggregateRoot, IAuditable
{
    public Guid FacultyId { get; set; }
    public string MajorName { get; set; } = default!;
    public string MajorCode { get; set; } = default!;
    public Faculty Faculty { get; set; } = default!;
    public ICollection<SubjectMajor> SubjectMajors { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
