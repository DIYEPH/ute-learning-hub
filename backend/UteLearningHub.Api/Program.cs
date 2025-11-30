using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using UteLearningHub.Api.BackgroundServices;
using UteLearningHub.Api.ConfigurationOptions;
using UteLearningHub.Api.Hubs;
using UteLearningHub.Api.Middleware;
using UteLearningHub.Api.Services;
using UteLearningHub.Application;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Infrastructure;
using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Infrastructure.DateTimes;
using UteLearningHub.Persistence;
using UteLearningHub.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();


// Add services to the container.

var services = builder.Services;
var configurations = builder.Configuration;

var appSettings = new AppSettings();
configurations.Bind(appSettings);

services.Configure<AppSettings>(configurations);
services.Configure<JwtOptions>(configurations.GetSection(JwtOptions.SectionName));
services.Configure<MicrosoftAuthOptions>(configurations.GetSection(MicrosoftAuthOptions.SectionName));
services.Configure<AmazonS3Options>(configurations.GetSection(AmazonS3Options.SectionName));
services.Configure<FileStorageOptions>(configurations.GetSection(FileStorageOptions.SectionName));
services.Configure<KafkaOptions>(configurations.GetSection(KafkaOptions.SectionName));

services.AddApplication()
    .AddPersistence(appSettings.ConnectionStrings.DefaultConnection)
    .AddInfrastructure(appSettings.Jwt);

services.AddDateTimeProvider();

// Configure File Storage: S3 or Local
var s3Options = configurations.GetSection(AmazonS3Options.SectionName).Get<AmazonS3Options>();
if (s3Options != null && !string.IsNullOrEmpty(s3Options.AccessKeyId) && !string.IsNullOrEmpty(s3Options.S3BucketName))
{
    // Use AWS S3
    services.AddS3FileStorage(s3Options);
}
else
{
    // Use Local Storage
    services.AddLocalFileStorage();
}

// Add Controllers
services.AddControllers();
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        
        // Thêm Bearer JWT Security Scheme
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT token. If not provided, token will be read from cookies."
        };
        
        return Task.CompletedTask;
    });
});
services.AddSignalR();

services.AddSingleton<IMessageHubService, SignalRMessageHubService>();
services.AddHostedService<KafkaMessageConsumerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseStaticFiles();

// Middleware để đọc JWT token từ cookies nếu không có trong Authorization header
app.UseMiddleware<JwtCookieMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
