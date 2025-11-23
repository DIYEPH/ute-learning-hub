using System.IO;
using Microsoft.Extensions.Configuration;
using UteLearningHub.Application.Services.FileStorage;

namespace UteLearningHub.Infrastructure.Services.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public FileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "/uploads";
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_basePath, uniqueFileName);
        var fileUrl = $"{_baseUrl}/{uniqueFileName}";

        using (var fileStreamWriter = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);
        }

        return fileUrl;
    }

    public Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_basePath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task<Stream?> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = Path.GetFileName(fileUrl);
            var filePath = Path.Combine(_basePath, fileName);
            
            if (File.Exists(filePath))
            {
                return Task.FromResult<Stream?>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
            }
            
            return Task.FromResult<Stream?>(null);
        }
        catch
        {
            return Task.FromResult<Stream?>(null);
        }
    }
}