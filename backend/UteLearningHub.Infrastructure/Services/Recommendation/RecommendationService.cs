using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly RecommendationOptions _options;
    private readonly ILogger<RecommendationService> _logger;

    public RecommendationService(
        HttpClient httpClient,
        IOptions<RecommendationOptions> options,
        ILogger<RecommendationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.AiServiceBaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
    }

    public async Task<RecommendationResponse> GetRecommendationsAsync(
        float[] userVector,
        IReadOnlyList<ConversationVectorData> conversationVectors,
        int topK = 10,
        float minSimilarity = 0.3f,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new RecommendationRequestDto
            {
                UserVector = userVector,
                ConversationVectors = conversationVectors.Select(cv => new ConversationVectorDto
                {
                    Id = cv.Id.ToString(),
                    Vector = cv.Vector
                }).ToList(),
                TopK = topK,
                MinSimilarity = minSimilarity
            };

            _logger.LogInformation("Calling AI service at {AiUrl}/recommend with {ConversationCount} conversations, topK={TopK}, minSimilarity={MinSimilarity}",
                _options.AiServiceBaseUrl, conversationVectors.Count, topK, minSimilarity);

            var response = await _httpClient.PostAsJsonAsync(
                "/recommend",
                request,
                cancellationToken);

            _logger.LogInformation("AI service responded with status {StatusCode}", response.StatusCode);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RecommendationResponseDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            if (result == null)
            {
                _logger.LogWarning("AI service returned null response");
                return new RecommendationResponse(
                    Array.Empty<RecommendationItem>(),
                    0,
                    0);
            }

            var recommendations = result.Recommendations?.Select((r, index) => new RecommendationItem(
                Guid.Parse(r.ConversationId),
                r.Similarity,
                r.Rank)).ToList() ?? new List<RecommendationItem>();

            return new RecommendationResponse(
                recommendations,
                result.TotalProcessed,
                result.ProcessingTimeMs);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AI recommendation service unavailable, returning empty recommendations");
            return new RecommendationResponse(Array.Empty<RecommendationItem>(), 0, 0);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "AI recommendation service timeout, returning empty recommendations");
            return new RecommendationResponse(Array.Empty<RecommendationItem>(), 0, 0);
        }
    }

    private record RecommendationRequestDto
    {
        public float[] UserVector { get; set; } = Array.Empty<float>();
        public List<ConversationVectorDto> ConversationVectors { get; set; } = new();
        public int TopK { get; set; } = 10;
        public float MinSimilarity { get; set; } = 0.3f;
    }

    private record ConversationVectorDto
    {
        public string Id { get; set; } = string.Empty;
        public float[] Vector { get; set; } = Array.Empty<float>();
    }

    private record RecommendationResponseDto
    {
        public List<RecommendationItemDto>? Recommendations { get; set; }
        public int TotalProcessed { get; set; }
        public double ProcessingTimeMs { get; set; }
    }

    private record RecommendationItemDto
    {
        public string ConversationId { get; set; } = string.Empty;
        public float Similarity { get; set; }
        public int Rank { get; set; }
    }
}

