namespace UteLearningHub.Application.Services.Recommendation;

/// <summary>
/// Service for AI text embeddings (Sentence Transformer)
/// </summary>
public interface IEmbeddingService
{
    /// <summary>Vector dimension (384 for all-MiniLM-L6-v2)</summary>
    int Dim { get; }

    /// <summary>Calculate user vector from behavior (subjects + tags)</summary>
    Task<float[]> UserVectorAsync(UserVectorRequest req, CancellationToken ct = default);

    /// <summary>Calculate conversation vector from content (name + subject + tags)</summary>
    Task<float[]> ConvVectorAsync(ConvVectorRequest req, CancellationToken ct = default);
}

/// <summary>User behavior data for vector calculation</summary>
public record UserVectorRequest
{
    public List<string> Subjects { get; init; } = [];
    public List<float> SubjectWeights { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<float> TagWeights { get; init; } = [];
}

/// <summary>Conversation content for vector calculation</summary>
public record ConvVectorRequest
{
    public string Name { get; init; } = "";
    public string? Subject { get; init; }
    public List<string> Tags { get; init; } = [];
}
