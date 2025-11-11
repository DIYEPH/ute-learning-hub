namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class MicrosoftAuthOptions
{
    public const string SectionName = "Microsoft";
    public string ClientId { get; init; } = default!;
    public string TenantId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
}
