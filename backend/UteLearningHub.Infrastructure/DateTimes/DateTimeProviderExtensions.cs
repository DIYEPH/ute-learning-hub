using Microsoft.Extensions.DependencyInjection;
using UteLearningHub.CrossCuttingConcerns.DateTimes;

namespace UteLearningHub.Infrastructure.DateTimes;

public static class DateTimeProviderExtensions
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
