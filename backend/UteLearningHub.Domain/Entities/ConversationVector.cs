using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationVector : BaseEntity<Guid>
{
    public Guid ConversationId { get; set; }
    public Guid? SubjectId { get; set; }
    public ProfileVectorType VectorType { get; set; } = ProfileVectorType.ConversationTopic;
    public int VectorDimension { get; set; }
    public string EmbeddingJson { get; set; } = default!;
    public string? SourceDataJson { get; set; }
    public string? ModelVersion { get; set; }
    public SimilarityMetric SimilarityMetric { get; set; } = SimilarityMetric.Cosine;
    public DateTimeOffset CalculatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Conversation Conversation { get; set; } = default!;
    public Subject? Subject { get; set; }
}

