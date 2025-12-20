using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class ReportRepository : Repository<Report, Guid>, IReportRepository
{
    public ReportRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) 
        : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<Report?> GetByIdWithContentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Include(r => r.DocumentFile)
            .Include(r => r.Comment)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IList<Report>> GetRelatedPendingReportsAsync(
        Guid? documentFileId,
        Guid? commentId,
        CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Include(r => r.DocumentFile)
            .Include(r => r.Comment)
            .Where(r => !r.IsDeleted && r.Status == ContentStatus.PendingReview);

        if (documentFileId.HasValue)
            query = query.Where(r => r.DocumentFileId == documentFileId.Value);
        else if (commentId.HasValue)
            query = query.Where(r => r.CommentId == commentId.Value);
        else
            return [];

        return await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }


    public async Task<int> GetDailyInstantApproveCountAsync(
        Guid userId,
        DateTimeOffset today,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = today.Date;
        var endOfDay = startOfDay.AddDays(1);

        // Count reports where CreatedById == ReviewedById (self-approved) today
        return await GetQueryableSet()
            .Where(r => !r.IsDeleted
                && r.CreatedById == userId
                && r.ReviewedById == userId
                && r.Status == ContentStatus.Approved
                && r.CreatedAt >= startOfDay
                && r.CreatedAt < endOfDay)
            .CountAsync(cancellationToken);
    }

    public async Task<Report?> GetUserPendingReportAsync(
        Guid userId,
        Guid? documentFileId,
        Guid? commentId,
        CancellationToken cancellationToken = default)
    {
        return await GetQueryableSet()
            .Where(r => !r.IsDeleted
                && r.CreatedById == userId
                && r.Status == ContentStatus.PendingReview
                && ((documentFileId.HasValue && r.DocumentFileId == documentFileId.Value)
                    || (commentId.HasValue && r.CommentId == commentId.Value)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

