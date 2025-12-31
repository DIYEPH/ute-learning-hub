namespace UteLearningHub.Application.Services.Recommendation;

public interface IRecommendationService
{
    /// <summary>Gọi AI service để lấy danh sách nhóm học được gợi ý</summary>
    Task<RecommendationResponse> GetRecommendationsAsync(
        float[] userVector,
        IReadOnlyList<ConversationVectorData> conversationVectors,
        int topK = 10,
        float minSimilarity = 0.3f,
        CancellationToken cancellationToken = default);

    /// <summary>Gọi AI service để tìm users tương tự với conversation</summary>
    Task<SimilarUsersResponse> GetSimilarUsersAsync(
        float[] convVector,
        IReadOnlyList<UserVectorData> userVectors,
        int topK = 10,
        float minScore = 0.3f,
        CancellationToken cancellationToken = default);

    /// <summary>Cluster users tương tự để tạo proposal</summary>
    Task<ClusterUsersResponse> ClusterSimilarUsersAsync(
        IReadOnlyList<UserVectorData> userVectors,
        int minClusterSize = 5,
        CancellationToken cancellationToken = default);
}

public record ConversationVectorData(Guid Id, float[] Vector);
public record UserVectorData(Guid Id, float[] Vector);

public record RecommendationResponse(
    IReadOnlyList<RecommendationItem> Recommendations,
    int TotalProcessed,
    double ProcessingTimeMs);

public record RecommendationItem(Guid ConversationId, float Similarity, int Rank);

public record SimilarUsersResponse(
    IReadOnlyList<SimilarUserItem> Users,
    int TotalProcessed,
    double ProcessingTimeMs);

public record SimilarUserItem(Guid UserId, float Similarity, int Rank);

public record ClusterUsersResponse(
    IReadOnlyList<UserCluster> Clusters,
    int TotalProcessed,
    double ProcessingTimeMs);

public record UserCluster(
    int ClusterId,
    IReadOnlyList<ClusterMember> Members,
    float[] Centroid);

public record ClusterMember(Guid UserId, float SimilarityToCentroid);
