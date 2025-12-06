namespace UteLearningHub.Application.Services.Recommendation;

public interface IRecommendationService
{
    /// <summary>
    /// Gọi AI service để lấy danh sách nhóm học được gợi ý
    /// </summary>
    /// <param name="userVector">Vector của user</param>
    /// <param name="conversationVectors">Danh sách vectors của các conversations</param>
    /// <param name="topK">Số lượng recommendations tối đa</param>
    /// <param name="minSimilarity">Ngưỡng similarity tối thiểu</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Danh sách recommendations với similarity scores</returns>
    Task<RecommendationResponse> GetRecommendationsAsync(
        float[] userVector,
        IReadOnlyList<ConversationVectorData> conversationVectors,
        int topK = 10,
        float minSimilarity = 0.3f,
        CancellationToken cancellationToken = default);
}

public record ConversationVectorData(Guid Id, float[] Vector);

public record RecommendationResponse(
    IReadOnlyList<RecommendationItem> Recommendations,
    int TotalProcessed,
    double ProcessingTimeMs);

public record RecommendationItem(
    Guid ConversationId,
    float Similarity,
    int Rank);

