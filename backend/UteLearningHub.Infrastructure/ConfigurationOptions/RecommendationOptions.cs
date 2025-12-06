namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class RecommendationOptions
{
    public const string SectionName = "Recommendation";

    /// <summary>
    /// Base URL của AI Recommendation Service
    /// </summary>
    public string AiServiceBaseUrl { get; set; } = "http://localhost:8000";

    /// <summary>
    /// Timeout cho HTTP requests đến AI service (seconds)
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Số lượng recommendations mặc định
    /// </summary>
    public int DefaultTopK { get; set; } = 10;

    /// <summary>
    /// Ngưỡng similarity tối thiểu
    /// </summary>
    public float DefaultMinSimilarity { get; set; } = 0.3f;

    /// <summary>
    /// Vector dimension mặc định
    /// </summary>
    public int DefaultVectorDimension { get; set; } = 100;
}

