namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Connection string cho Redis (ví dụ: localhost:6379)
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Instance name prefix cho cache keys (ví dụ: "ute-learninghub:")
    /// </summary>
    public string InstanceName { get; set; } = "ute-learninghub:";

    /// <summary>
    /// Default expiration time cho cache entries (minutes)
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 15;
}

