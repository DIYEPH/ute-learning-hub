namespace UteLearningHub.Application.Services.Document;

public interface IPdfPageCountService
{
    /// <summary>
    /// Đọc số trang của file PDF từ stream
    /// </summary>
    /// <param name="fileStream">Stream của file PDF</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Số trang của PDF, hoặc null nếu không phải PDF hoặc không đọc được</returns>
    Task<int?> GetPageCountAsync(Stream fileStream, CancellationToken cancellationToken = default);
}

