namespace UteLearningHub.Application.Services.Recommendation;

public interface IRecommendationService
{
    Task<RecommendationResponse> GetRecommendationsAsync(
        float[] userVector,
        IReadOnlyList<ConversationVectorData> conversationVectors,
        int topK = 10,
        float minSimilarity = 0.3f,
        CancellationToken cancellationToken = default);

    Task<SimilarUsersResponse> GetSimilarUsersAsync(
        float[] convVector,
        IReadOnlyList<UserVectorData> userVectors,
        int topK = 10,
        float minScore = 0.3f,
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
