using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class UserDocumentProgressRepository : Repository<UserDocumentProgress, Guid>, IUserDocumentProgressRepository
{
    public UserDocumentProgressRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<UserDocumentProgress?> GetByUserAndDocumentFileAsync(Guid userId, Guid documentFileId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Where(p => p.UserId == userId && p.DocumentFileId == documentFileId);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IList<UserDocumentProgress>> GetByUserAndDocumentAsync(Guid userId, Guid documentId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Where(p => p.UserId == userId && p.DocumentId == documentId);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }
}

