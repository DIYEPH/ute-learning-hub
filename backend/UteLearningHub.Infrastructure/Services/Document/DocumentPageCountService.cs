using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentPageCountService : IDocumentPageCountService
{
    private readonly IPdfPageCountService _pdfPageCountService;
    private readonly DocxPageCountService _docxPageCountService;

    public DocumentPageCountService(
        IPdfPageCountService pdfPageCountService,
        DocxPageCountService docxPageCountService)
    {
        _pdfPageCountService = pdfPageCountService;
        _docxPageCountService = docxPageCountService;
    }

    public async Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return null;

        var mimeTypeLower = mimeType.ToLowerInvariant();

        // Image files - luôn là 1 trang
        if (mimeTypeLower.StartsWith("image/"))
        {
            return 1;
        }

        // PDF
        if (mimeTypeLower == "application/pdf" || mimeTypeLower.Contains("pdf"))
        {
            return await _pdfPageCountService.GetPageCountAsync(fileStream, cancellationToken);
        }

        // DOCX/DOC
        if (mimeTypeLower.Contains("word") ||
            mimeTypeLower.Contains("officedocument.wordprocessingml") ||
            mimeTypeLower.Contains("msword"))
        {
            return await _docxPageCountService.GetPageCountAsync(fileStream, mimeType, cancellationToken);
        }

        return null;
    }
}

