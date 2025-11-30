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
using UteLearningHub.Api.Binders;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

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

var s3Options = configurations.GetSection(AmazonS3Options.SectionName).Get<AmazonS3Options>();
if (s3Options != null && !string.IsNullOrEmpty(s3Options.AccessKeyId) && !string.IsNullOrEmpty(s3Options.S3BucketName))
{
    services.AddS3FileStorage(s3Options);
}
else
{
    services.AddLocalFileStorage();
}

services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new OptionalListModelBinderProvider());
});
services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        
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

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
