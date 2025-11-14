using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Persistence.ConfigurationOptions;

namespace UteLearningHub.Api.ConfigurationOptions;

public class AppSettings
{
    public ConnectionStringsOptions ConnectionStrings { get; set; } = default!;
    public JwtOptions Jwt { get; set; } = default!;
    public MicrosoftAuthOptions Microsoft { get; set; } = default!;
}