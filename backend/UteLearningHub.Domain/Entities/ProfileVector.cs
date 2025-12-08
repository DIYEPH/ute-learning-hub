namespace UteLearningHub.Domain.Entities;

public class ProfileVector
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EmbeddingJson { get; set; } = default!; // JSON array [0.1, 0.2, ...]
    public DateTimeOffset CalculatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
