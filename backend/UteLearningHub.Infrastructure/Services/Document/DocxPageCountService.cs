using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocxPageCountService : IDocumentPageCountService
{
    public Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken ct = default)
    {
        try
        {
            if (fileStream.CanSeek)
                fileStream.Position = 0;

            // Chỉ xử lý Word documents
            var mime = mimeType?.ToLowerInvariant() ?? "";
            if (!mime.Contains("word") && !mime.Contains("officedocument.wordprocessingml") && !mime.Contains("msword"))
                return Task.FromResult<int?>(null);

            using var doc = WordprocessingDocument.Open(fileStream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null)
                return Task.FromResult<int?>(null);

            // Đếm page breaks và sections
            var pageBreaks = body.Descendants<Break>().Count(b => b.Type?.Value == BreakValues.Page);
            var sections = body.Descendants<SectionProperties>().Count();
            var paragraphs = body.Descendants<Paragraph>().Count();

            // Ước tính số trang
            var pages = Math.Max(1, sections + pageBreaks);
            if (pages == 1 && paragraphs > 50)
                pages = Math.Max(1, (int)Math.Ceiling(paragraphs / 35.0));

            return Task.FromResult<int?>(pages);
        }
        catch
        {
            return Task.FromResult<int?>(null);
        }
    }
}
