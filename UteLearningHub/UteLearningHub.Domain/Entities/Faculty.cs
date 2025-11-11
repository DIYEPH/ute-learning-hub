using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Faculty : BaseEntity<Guid>, IAggregateRoot, IAuditable
{
    public string FacultyName { get; set; } = default!;
    public string FacultyCode { get; set; } = default!;
    public ICollection<Major> Majors { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
