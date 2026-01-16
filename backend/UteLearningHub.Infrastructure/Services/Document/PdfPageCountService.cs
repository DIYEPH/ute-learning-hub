using UglyToad.PdfPig;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class PdfPageCountService : IPdfPageCountService
{
    public Task<int?> GetPageCountAsync(Stream fileStream, CancellationToken ct = default)
    {
        try
        {
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            using var doc = PdfDocument.Open(fileStream);
            return Task.FromResult<int?>(doc.NumberOfPages);
        }
        catch
        {
            return Task.FromResult<int?>(null);
        }
    }
}