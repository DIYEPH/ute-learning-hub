using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class DocumentRepository : Repository<Document, Guid>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<Document?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Include(d => d.Subject)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentAuthors)
                .ThenInclude(da => da.Author)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.File)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.CoverFile)
            .Where(d => d.Id == id && !d.IsDeleted);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<Document> GetQueryableWithIncludes()
    {
        return GetQueryableSet()
            .Include(d => d.Subject)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentAuthors)
                .ThenInclude(da => da.Author)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.File)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.CoverFile)
            .AsSplitQuery();
    }

    public async Task<Guid?> GetDocumentIdByDocumentFileIdAsync(Guid documentFileId, CancellationToken cancellationToken = default)
    {
        var result = await GetQueryableSet()
            .Where(d => !d.IsDeleted && d.DocumentFiles.Any(df => df.Id == documentFileId))
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<Document?> GetByFileIdAsync(Guid fileId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Where(d => !d.IsDeleted && d.DocumentFiles.Any(df => df.FileId == fileId && !df.IsDeleted));

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<DocumentFile?> GetDocumentFileByIdAsync(Guid documentFileId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<DocumentFile>()
            .Where(df => df.Id == documentFileId && !df.IsDeleted);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddDocumentFileAsync(DocumentFile documentFile, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<DocumentFile>().AddAsync(documentFile, cancellationToken);
    }

    public async Task<bool> IsDocumentFileUsedElsewhereAsync(Guid fileId, Guid excludeDocumentFileId, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(d => !d.IsDeleted)
            .SelectMany(d => d.DocumentFiles)
            .AnyAsync(df => df.FileId == fileId && !df.IsDeleted && df.Id != excludeDocumentFileId, cancellationToken);
    }
}
