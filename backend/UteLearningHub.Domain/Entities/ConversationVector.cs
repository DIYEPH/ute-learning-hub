using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Domain.Entities;

public class ConversationVector
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? SubjectId { get; set; }  // Optional: để filter
    public ProfileVectorType VectorType { get; set; } = ProfileVectorType.ConversationTopic;
    public int VectorDimension { get; set; }
    public string EmbeddingJson { get; set; } = default!;
    public string? SourceDataJson { get; set; }  // Optional
    public DateTimeOffset CalculatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Conversation Conversation { get; set; } = default!;
    public Subject? Subject { get; set; }
}