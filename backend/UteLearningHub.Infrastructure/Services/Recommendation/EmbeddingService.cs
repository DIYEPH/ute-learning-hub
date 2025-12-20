using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Infrastructure.Services.Recommendation;

/// <summary>
/// Calls Python AI service for text embeddings
/// </summary>
public class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly ILogger<EmbeddingService> _log;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public int Dim => 384;

    public EmbeddingService(HttpClient http, IOptions<RecommendationOptions> opts, ILogger<EmbeddingService> log)
    {
        _http = http;
        _log = log;
        _http.BaseAddress = new Uri(opts.Value.AiServiceBaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(opts.Value.RequestTimeoutSeconds);
    }

    public async Task<float[]> UserVectorAsync(UserVectorRequest req, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                subjects = req.Subjects,
                subjectWeights = req.SubjectWeights,
                tags = req.Tags,
                tagWeights = req.TagWeights
            };

            var res = await _http.PostAsJsonAsync("/vector/user", payload, ct);
            res.EnsureSuccessStatusCode();

            var data = await res.Content.ReadFromJsonAsync<VectorResponse>(Json, ct);
            return data?.Vector ?? new float[Dim];
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to calculate user vector");
            return new float[Dim];
        }
    }

    public async Task<float[]> ConvVectorAsync(ConvVectorRequest req, CancellationToken ct = default)
    {
        try
        {
            var payload = new
            {
                name = req.Name,
                subject = req.Subject,
                tags = req.Tags
            };

            var res = await _http.PostAsJsonAsync("/vector/conv", payload, ct);
            res.EnsureSuccessStatusCode();

            var data = await res.Content.ReadFromJsonAsync<VectorResponse>(Json, ct);
            return data?.Vector ?? new float[Dim];
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Failed to calculate conv vector");
            return new float[Dim];
        }
    }

    private record VectorResponse(float[] Vector, int Dim);
}
