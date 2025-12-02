using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Infrastructure.Services.Authentication;
using UteLearningHub.Infrastructure.Services.Comment;
using UteLearningHub.Infrastructure.Services.Identity;
using UteLearningHub.Infrastructure.Services.Message;
using UteLearningHub.Infrastructure.Services.User;
using UteLearningHub.Infrastructure.Services.FileStorage;
using Amazon.S3;
using Microsoft.Extensions.Options;

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

            // Đọc token từ cookie nếu không có trong Authorization header
            // Middleware sẽ xử lý việc này, nhưng đây là fallback
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Chỉ đọc từ cookie nếu chưa có token trong header
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        var token = context.Request.Cookies["access_token"] 
                                 ?? context.Request.Cookies["jwt_token"] 
                                 ?? context.Request.Cookies["token"]
                                 ?? context.Request.Cookies["auth_token"];
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddSingleton<IMessageQueueProducer, KafkaMessageProducer>();
        services.AddSingleton<IConnectionTracker, ConnectionTrackerService>();
        services.AddScoped<IUserConversationService, UserConversationService>();
        services.AddScoped<IConversationSystemMessageService, ConversationSystemMessageService>();

        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
    public static IServiceCollection AddS3FileStorage(this IServiceCollection services, AmazonS3Options s3Options)
    {
        // Configure AWS S3 Client
        services.AddSingleton<IAmazonS3>(sp =>
        {
            var config = new AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(s3Options.Region)
            };
            return new AmazonS3Client(s3Options.AccessKeyId, s3Options.SecretAccessKey, config);
        });

        // Register options
        services.AddSingleton(Options.Create(s3Options));

        // Register S3 File Storage Service
        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, FileStorageService>();
        return services;
    }
}
