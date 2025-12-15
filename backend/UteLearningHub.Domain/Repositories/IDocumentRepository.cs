using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IDocumentRepository : IRepository<Document, Guid>
{
    Task<Document?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    IQueryable<Document> GetQueryableWithIncludes();
    Task<Guid?> GetDocumentIdByDocumentFileIdAsync(Guid documentFileId, CancellationToken cancellationToken = default);
    Task<Document?> GetByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<DocumentFile?> GetDocumentFileByIdAsync(Guid documentFileId, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task AddDocumentFileAsync(DocumentFile documentFile, CancellationToken cancellationToken = default);
    Task<bool> IsDocumentFileUsedElsewhereAsync(Guid fileId, Guid excludeDocumentFileId, CancellationToken cancellationToken = default);
    Task<int> GetPendingFilesCountAsync(CancellationToken cancellationToken = default);
}
