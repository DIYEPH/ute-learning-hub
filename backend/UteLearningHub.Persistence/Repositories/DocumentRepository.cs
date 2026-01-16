using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class DocumentRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : Repository<Document, Guid>(dbContext, dateTimeProvider), IDocumentRepository
{
    public async Task<Document?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken ct = default)
    {
        var query = GetQueryableSet()
            .Include(d => d.Subject)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentAuthors).ThenInclude(da => da.Author)
            .Include(d => d.DocumentFiles).ThenInclude(df => df.File)
            .Include(d => d.DocumentFiles).ThenInclude(df => df.CoverFile)
            .Where(d => d.Id == id);

        if (disableTracking) query = query.AsNoTracking();
        return await query.AsSplitQuery().FirstOrDefaultAsync(ct);
    }

    public async Task<Document?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await GetQueryableSet()
            .Include(d => d.DocumentTags)
            .Include(d => d.DocumentAuthors)
            .Include(d => d.CoverFile)
            .Where(d => d.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public IQueryable<Document> GetQueryableWithIncludes()
    {
        return GetQueryableSet()
            .Include(d => d.Subject)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags).ThenInclude(dt => dt.Tag)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentAuthors).ThenInclude(da => da.Author)
            .Include(d => d.DocumentFiles).ThenInclude(df => df.File)
            .Include(d => d.DocumentFiles).ThenInclude(df => df.CoverFile)
            .Include(d => d.Reviews)
            .AsSplitQuery();
    }

    public async Task<Guid?> GetIdByDocumentFileIdAsync(Guid documentFileId, CancellationToken ct = default)
    {
        return await GetQueryableSet()
            .Where(d => d.DocumentFiles.Any(df => df.Id == documentFileId))
            .Select(d => d.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Document?> GetByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken ct = default)
    {
        var query = GetQueryableSet().Where(d => d.DocumentFiles.Any(df => df.FileId == fileId));
        if (disableTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<DocumentFile?> GetDocumentFileByIdAsync(Guid documentFileId, bool disableTracking = false, CancellationToken ct = default)
    {
        var query = _dbContext.Set<DocumentFile>().Where(df => df.Id == documentFileId);
        if (disableTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<DocumentFile?> GetDocumentFileByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken ct = default)
    {
        var query = _dbContext.Set<DocumentFile>().Where(df => df.FileId == fileId);
        if (disableTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(ct);
    }

    public void AddDocumentFile(DocumentFile documentFile) => _dbContext.Set<DocumentFile>().Add(documentFile);

    public void UpdateDocumentFile(DocumentFile documentFile) => _dbContext.Set<DocumentFile>().Update(documentFile);

    public async Task<int> GetPendingFilesCountAsync(CancellationToken ct = default)
    {
        return await _dbContext.Set<DocumentFile>()
            .Where(df => df.Status == ContentStatus.PendingReview)
            .CountAsync(ct);
    }
}
