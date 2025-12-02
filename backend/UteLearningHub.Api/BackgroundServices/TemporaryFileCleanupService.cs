using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Infrastructure.ConfigurationOptions;

namespace UteLearningHub.Api.BackgroundServices;

public class TemporaryFileCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TemporaryFileCleanupService> _logger;
    private readonly TemporaryFileCleanupOptions _options;

    public TemporaryFileCleanupService(
        IServiceScopeFactory scopeFactory,
        IOptions<TemporaryFileCleanupOptions> options,
        ILogger<TemporaryFileCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Temporary file cleanup service is disabled.");
            return;
        }

        _logger.LogInformation("Temporary file cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanTemporaryFilesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cleaning temporary files.");
            }

            try
            {
                await Task.Delay(_options.CleanupInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Temporary file cleanup service stopped.");
    }

    private async Task CleanTemporaryFilesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var threshold = dateTimeProvider.OffsetNow - _options.FileTtl;

        var expiredFiles = await fileRepository.GetQueryableSet()
            .Where(f => f.IsTemporary && f.CreatedAt < threshold && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!expiredFiles.Any())
            return;

        foreach (var file in expiredFiles)
        {
            try
            {
                await fileStorageService.DeleteFileAsync(file.FileUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {FileUrl} from storage.", file.FileUrl);
            }

            await fileRepository.DeleteAsync(file, cancellationToken);
        }

        await fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cleaned {Count} temporary files.", expiredFiles.Count);
    }
}


