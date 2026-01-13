
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UteLearningHub.Application.Services.ContentModeration;

namespace UteLearningHub.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddSingleton<IProfanityFilterService, ProfanityFilterService>();
        return services;
    }
}
