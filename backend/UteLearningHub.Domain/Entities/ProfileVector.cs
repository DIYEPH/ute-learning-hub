using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ProfileVector
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SubjectId { get; set; }  // Optional: để filter theo môn học
    public ProfileVectorType VectorType { get; set; }
    public int VectorDimension { get; set; }
    public string EmbeddingJson { get; set; } = default!; // JSON array [0.1, 0.2, ...]
    public string? SourceDataJson { get; set; }  // Optional: metadata để debug
    public DateTimeOffset CalculatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Subject? Subject { get; set; }
}
