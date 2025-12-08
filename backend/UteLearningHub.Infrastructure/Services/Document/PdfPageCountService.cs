using UglyToad.PdfPig;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class PdfPageCountService : IPdfPageCountService
{
    public Task<int?> GetPageCountAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        try
        {
            // Reset stream position để đảm bảo đọc từ đầu
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            using var document = PdfDocument.Open(fileStream);
            var pageCount = document.NumberOfPages;

            return Task.FromResult<int?>(pageCount);
        }
        catch
        {
            // Không phải PDF hoặc file bị lỗi
            return Task.FromResult<int?>(null);
        }
    }
}

