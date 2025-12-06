namespace UteLearningHub.Application.Services.Document;

public interface IDocumentPageCountService
{
    /// <summary>
    /// Đọc số trang của file từ stream
    /// </summary>
    /// <param name="fileStream">Stream của file</param>
    /// <param name="mimeType">MIME type của file (ví dụ: application/pdf, application/vnd.openxmlformats-officedocument.wordprocessingml.document)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Số trang của file, hoặc null nếu không đọc được</returns>
    Task<int?> GetPageCountAsync(Stream fileStream, string mimeType, CancellationToken cancellationToken = default);
}

