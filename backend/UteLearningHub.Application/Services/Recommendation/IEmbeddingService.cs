namespace UteLearningHub.Application.Services.Recommendation;
public interface IEmbeddingService
{
    /// (384 for all-MiniLM-L6-v2)
    int Dim { get; }

    /// (major + subjects + tags)
    Task<float[]> UserVectorAsync(UserVectorRequest req, CancellationToken ct = default);

    /// (name + subject + tags)
    Task<float[]> ConvVectorAsync(ConvVectorRequest req, CancellationToken ct = default);
}
public record UserVectorRequest
{
    public List<string> Subjects { get; init; } = [];
    public List<float> SubjectWeights { get; init; } = [];
    public List<string> Tags { get; init; } = [];
    public List<float> TagWeights { get; init; } = [];
}
public record ConvVectorRequest
{
    public string Name { get; init; } = "";
    public string? Subject { get; init; }
    public List<string> Tags { get; init; } = [];
}
