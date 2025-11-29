namespace UteLearningHub.Domain.Entities;

public class SubjectMajor
{
    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = default!;
    
    public Guid MajorId { get; set; }
    public Major Major { get; set; } = default!;
}