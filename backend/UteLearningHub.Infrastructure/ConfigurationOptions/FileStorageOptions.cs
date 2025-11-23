namespace UteLearningHub.Infrastructure.ConfigurationOptions;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string BasePath { get; init; } = default!;
    public string BaseUrl { get; init; } = "/uploads";
}
