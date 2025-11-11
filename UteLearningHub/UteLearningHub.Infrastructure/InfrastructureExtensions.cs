using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Infrastructure.Services.Authentication;
using UteLearningHub.Infrastructure.Services.Identity;

namespace UteLearningHub.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, JwtOptions jwtOptions)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
