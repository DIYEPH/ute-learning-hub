using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly HttpClient _http;
    private readonly ILogger<RecommendationService> _log;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public RecommendationService(HttpClient http, IOptions<RecommendationOptions> opts, ILogger<RecommendationService> log)
    {
        _http = http;
        _log = log;
        _http.BaseAddress = new Uri(opts.Value.AiServiceBaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(opts.Value.RequestTimeoutSeconds);
    }

    // Gợi ý conversations cho user
    public async Task<RecommendationResponse> GetRecommendationsAsync(
        float[] userVector,
        IReadOnlyList<ConversationVectorData> conversationVectors,
        int topK = 10,
        float minSimilarity = 0.3f,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                UserVector = userVector,
                ConversationVectors = conversationVectors.Select(cv => new { Id = cv.Id.ToString(), cv.Vector }).ToList(),
                TopK = topK,
                MinSimilarity = minSimilarity
            };

            var res = await _http.PostAsJsonAsync("/recommend", request, ct);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<RecommendationResponseDto>(Json, ct);
            if (result == null)
                return new RecommendationResponse([], 0, 0);

            var items = result.Recommendations?.Select((r, i) => new RecommendationItem(
                Guid.Parse(r.ConversationId), r.Similarity, r.Rank)).ToList() ?? [];

            return new RecommendationResponse(items, result.TotalProcessed, result.ProcessingTimeMs);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "AI recommendation service failed");
            return new RecommendationResponse([], 0, 0);
        }
    }

    // Tìm users phù hợp với conversation
    public async Task<SimilarUsersResponse> GetSimilarUsersAsync(
        float[] convVector,
        IReadOnlyList<UserVectorData> userVectors,
        int topK = 10,
        float minScore = 0.3f,
        CancellationToken ct = default)
    {
        try
        {
            var request = new
            {
                ConvVector = convVector,
                UserVectors = userVectors.Select(uv => new { Id = uv.Id.ToString(), uv.Vector }).ToList(),
                TopK = topK,
                MinScore = minScore
            };

            var res = await _http.PostAsJsonAsync("/similar/users", request, ct);
            res.EnsureSuccessStatusCode();

            var result = await res.Content.ReadFromJsonAsync<SimilarUsersResponseDto>(Json, ct);
            if (result == null)
                return new SimilarUsersResponse([], 0, 0);

            var users = result.Users?.Select(u => new SimilarUserItem(
                Guid.Parse(u.UserId), u.Similarity, u.Rank)).ToList() ?? [];

            return new SimilarUsersResponse(users, result.TotalProcessed, result.ProcessingTimeMs);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "AI similar users service failed");
            return new SimilarUsersResponse([], 0, 0);
        }
    }

    // DTOs
    private record RecommendationResponseDto(List<RecommendationItemDto>? Recommendations, int TotalProcessed, double ProcessingTimeMs);
    private record RecommendationItemDto(string ConversationId, float Similarity, int Rank);
    private record SimilarUsersResponseDto(List<SimilarUserItemDto>? Users, int TotalProcessed, double ProcessingTimeMs);
    private record SimilarUserItemDto(string UserId, float Similarity, int Rank);
}
