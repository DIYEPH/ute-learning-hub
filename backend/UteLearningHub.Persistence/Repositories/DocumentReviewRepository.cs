using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace UteLearningHub.Persistence.Repositories;

public class DocumentReviewRepository : Repository<DocumentReview, Guid>, IDocumentReviewRepository
{
    public DocumentReviewRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) 
        : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<DocumentReview?> GetByDocumentIdAndUserIdAsync(Guid documentId, Guid userId, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Where(dr => dr.DocumentId == documentId && dr.CreatedById == userId && !dr.IsDeleted);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
