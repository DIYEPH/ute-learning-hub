using System.Text;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Cache;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Email;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.Settings;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Application.Services.Author;
using UteLearningHub.Application.Services.Conversation;
using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Infrastructure.Services.Authentication;
using UteLearningHub.Infrastructure.Services.Cache;
using UteLearningHub.Infrastructure.Services.Comment;
using UteLearningHub.Infrastructure.Services.Document;
using UteLearningHub.Infrastructure.Services.Email;
using UteLearningHub.Infrastructure.Services.File;
using UteLearningHub.Infrastructure.Services.FileStorage;
using UteLearningHub.Infrastructure.Services.Identity;
using UteLearningHub.Infrastructure.Services.Message;
using UteLearningHub.Infrastructure.Services.Recommendation;
using UteLearningHub.Infrastructure.Services.Settings;
using UteLearningHub.Infrastructure.Services.TrustScore;
using UteLearningHub.Infrastructure.Services.User;
using UteLearningHub.Infrastructure.Services.Author;
using UteLearningHub.Infrastructure.Services.Conversation;

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

            // Đọc token từ query string nếu không có trong Authorization header
            // Query string được dùng cho iframe/img tags (không thể gửi header)
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.Query["access_token"].FirstOrDefault();
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IPasswordResetLinkBuilder, PasswordResetLinkBuilder>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddSingleton<IMessageQueueProducer, KafkaMessageProducer>();
        services.AddSingleton<IConnectionTracker, ConnectionTrackerService>();
        services.AddScoped<IUserConversationService, UserConversationService>();
        services.AddScoped<IConversationSystemMessageService, ConversationSystemMessageService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();

        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileUsageService, FileUsageService>();

        // Recommendation services
        services.AddScoped<IVectorCalculationService, VectorCalculationService>();
        services.AddScoped<IUserDataRepository, UserDataRepository>();
        services.AddScoped<IVectorMaintenanceService, VectorMaintenanceService>();
        services.AddHttpClient<IRecommendationService, RecommendationService>();

        // Trust Score service
        services.AddScoped<ITrustScoreService, TrustScoreService>();

        // Document services
        services.AddScoped<IPdfPageCountService, PdfPageCountService>();
        services.AddScoped<DocxPageCountService>();
        services.AddScoped<IDocumentPageCountService, DocumentPageCountService>();
        services.AddScoped<IDocumentQueryService, DocumentQueryService>();

        // Author services
        services.AddScoped<IAuthorQueryService, AuthorQueryService>();

        // Conversation services
        services.AddScoped<IConversationQueryService, ConversationQueryService>();

        // Email service
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    public static IServiceCollection AddCacheService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.SectionName).Get<RedisOptions>();
        var useRedis = redisOptions != null && !string.IsNullOrEmpty(redisOptions.ConnectionString);

        if (useRedis)
        {
            // Register Redis
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                return ConnectionMultiplexer.Connect(options.ConnectionString);
            });

            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            // Fallback to MemoryCache
            services.AddMemoryCache();
            services.AddScoped<ICacheService, MemoryCacheService>();
        }

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
