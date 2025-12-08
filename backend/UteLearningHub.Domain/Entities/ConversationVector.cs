namespace UteLearningHub.Domain.Entities;

public class ConversationVector
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string EmbeddingJson { get; set; } = default!;
    public DateTimeOffset CalculatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Conversation Conversation { get; set; } = default!;
}