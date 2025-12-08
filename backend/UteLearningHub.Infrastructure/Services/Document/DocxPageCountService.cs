using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocxPageCountService : IDocumentPageCountService
{
    public Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Reset stream position để đảm bảo đọc từ đầu
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Chỉ xử lý DOCX và DOC
            var mimeTypeLower = mimeType?.ToLowerInvariant() ?? "";
            if (!mimeTypeLower.Contains("word") &&
                !mimeTypeLower.Contains("officedocument.wordprocessingml") &&
                !mimeTypeLower.Contains("msword"))
            {
                return Task.FromResult<int?>(null);
            }

            using var wordDocument = WordprocessingDocument.Open(fileStream, false);
            var mainPart = wordDocument.MainDocumentPart;
            if (mainPart == null)
            {
                return Task.FromResult<int?>(null);
            }

            var body = mainPart.Document?.Body;
            if (body == null)
            {
                return Task.FromResult<int?>(null);
            }

            // Đếm số page breaks và section breaks
            var pageBreaks = body.Descendants<Break>().Count(b => b.Type?.Value == BreakValues.Page);
            var sections = body.Descendants<SectionProperties>().Count();

            // Đếm số paragraphs để ước tính thêm
            var paragraphs = body.Descendants<Paragraph>().Count();

            // Ước tính số trang: 
            // - Mỗi section thường là 1 trang
            // - Mỗi page break là 1 trang mới
            // - Nếu có nhiều paragraphs (>50) thì có thể có nhiều trang hơn
            var estimatedPages = Math.Max(1, sections + pageBreaks);

            // Nếu có nhiều paragraphs nhưng không có page breaks, ước tính dựa trên số paragraphs
            if (estimatedPages == 1 && paragraphs > 50)
            {
                // Giả sử mỗi 30-40 paragraphs là 1 trang
                estimatedPages = Math.Max(1, (int)Math.Ceiling(paragraphs / 35.0));
            }

            return Task.FromResult<int?>(estimatedPages);
        }
        catch
        {
            // Không phải DOCX/DOC hoặc file bị lỗi
            return Task.FromResult<int?>(null);
        }
    }
}

