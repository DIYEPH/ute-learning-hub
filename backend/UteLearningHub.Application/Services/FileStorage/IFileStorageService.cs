namespace UteLearningHub.Application.Services.FileStorage;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<bool> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<Stream?> GetFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}