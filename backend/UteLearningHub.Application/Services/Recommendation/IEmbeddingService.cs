namespace UteLearningHub.Application.Services.Recommendation;
public interface IEmbeddingService
{
    /// (384 for all-MiniLM-L6-v2)
    int Dim { get; }

    /// Calculate UserVector(subjects + tags)
    Task<float[]> UserVectorAsync(UserVectorRequest req, CancellationToken ct = default);

    /// Calculate ConversationVector(name + subject + tags)
    Task<float[]> ConvVectorAsync(ConvVectorRequest req, CancellationToken ct = default);
}

/// User behavior data 
public record UserVectorRequest
{
    public List<string> Subjects { get; init; } = [];
    public List<float> SubjectWeights { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<float> TagWeights { get; init; } = [];
}

/// Conversation vector calculation
public record ConvVectorRequest
{
    public string Name { get; init; } = "";
    public string? Subject { get; init; }
    public List<string> Tags { get; init; } = [];
}
