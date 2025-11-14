using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ProfileVector : BaseEntity<Guid>
{
    public Guid SubjectId { get; set; }
    public Guid UserId { get; set; }
    public string EmbeddingJson { get; set; } = default!;
    public Subject Subject { get; set; } = default!;
}
