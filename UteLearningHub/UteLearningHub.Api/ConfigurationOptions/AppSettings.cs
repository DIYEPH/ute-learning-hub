using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Api.ConfigurationOptions;

public class AppSettings
{
    public ConnectionStringsOptions ConnectionStrings { get; set; } = default!;
}