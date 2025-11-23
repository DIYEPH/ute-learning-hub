using UteLearningHub.Api.ConfigurationOptions;
using UteLearningHub.Api.Middleware;
using UteLearningHub.Application;
using UteLearningHub.Infrastructure;
using UteLearningHub.Infrastructure.ConfigurationOptions;
using UteLearningHub.Infrastructure.DateTimes;
using UteLearningHub.Persistence;
using UteLearningHub.Persistence.Seeders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var services = builder.Services;
var configurations = builder.Configuration;

var appSettings = new AppSettings();
configurations.Bind(appSettings);

services.Configure<AppSettings>(configurations);
services.Configure<JwtOptions>(configurations.GetSection(JwtOptions.SectionName));
services.Configure<MicrosoftAuthOptions>(configurations.GetSection(MicrosoftAuthOptions.SectionName));
services.Configure<AmazonS3Options>(configurations.GetSection(AmazonS3Options.SectionName));

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
services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();

    app.MapOpenApi();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
