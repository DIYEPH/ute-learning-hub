using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace UteLearningHub.Persistence.Repositories;

public class DocumentRepository : Repository<Document, Guid>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<Document?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Include(d => d.Subject!)
                .ThenInclude(s => s.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
                        .ThenInclude(m => m.Faculty)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.File)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.File)
            .Where(d => d.Id == id);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<Document> GetQueryableWithIncludes()
    {
        return GetQueryableSet()
            .Include(d => d.Subject!)
                .ThenInclude(s => s.SubjectMajors)
                    .ThenInclude(sm => sm.Major)
                        .ThenInclude(m => m.Faculty)
            .Include(d => d.Type)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .Include(d => d.File)
            .Include(d => d.CoverFile)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.File)
            .Include(d => d.DocumentFiles)
                .ThenInclude(df => df.CoverFile);
    }
}
