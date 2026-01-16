using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentPageCountService(
    IPdfPageCountService pdfService,
    DocxPageCountService docxService) : IDocumentPageCountService
{
    public async Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return null;

        var mime = mimeType.ToLowerInvariant();

        if (mime.StartsWith("image/"))
            return 1;

        if (mime == "application/pdf" || mime.Contains("pdf"))
            return await pdfService.GetPageCountAsync(fileStream, ct);

        // chưa hỗ trợ cái này :))
        if (mime.Contains("word") || mime.Contains("officedocument.wordprocessingml") || mime.Contains("msword"))
            return await docxService.GetPageCountAsync(fileStream, mimeType, ct);

        return null;
    }
}
