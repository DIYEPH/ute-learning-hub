using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class DocumentReviewRepository : Repository<DocumentReview, Guid>, IDocumentReviewRepository
{
    public DocumentReviewRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<DocumentReview?> GetByIdAndUserIdAsync(Guid documentFileId, Guid userId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
           .Where(dr => dr.DocumentFileId == documentFileId && dr.CreatedById == userId);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
