using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IDocumentRepository : IRepository<Document, Guid>
{
    Task<Document?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<Document> GetQueryableWithIncludes();
    Task<Guid?> GetIdByDocumentFileIdAsync(Guid documentFileId, CancellationToken cancellationToken = default);
    Task<Document?> GetByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<DocumentFile?> GetDocumentFileByIdAsync(Guid documentFileId, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<DocumentFile?> GetDocumentFileByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken cancellationToken = default);
    void AddDocumentFile(DocumentFile documentFile);
    void UpdateDocumentFile(DocumentFile documentFile);
    Task<int> GetPendingFilesCountAsync(CancellationToken cancellationToken = default);
}
