namespace UteLearningHub.Application.Services.Document;

public interface IDocumentPageCountService
{
    Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken cancellationToken = default);
}

