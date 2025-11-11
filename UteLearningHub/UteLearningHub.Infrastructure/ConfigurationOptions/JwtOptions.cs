namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int ExpiryMinutes { get; init; }
    public int RefreshTokenExpiryDays { get; init; }
}
