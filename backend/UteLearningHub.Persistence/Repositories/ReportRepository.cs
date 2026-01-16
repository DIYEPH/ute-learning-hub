using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class ReportRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    : Repository<Report, Guid>(dbContext, dateTimeProvider), IReportRepository
{
    public async Task<Report?> GetByIdWithContentAsync(Guid id, CancellationToken ct = default)
    {
        return await GetQueryableSet()
            .Include(r => r.DocumentFile)
            .Include(r => r.Comment)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IList<Report>> GetRelatedPendingReportsAsync(
        Guid? documentFileId,
        Guid? commentId,
        ReportReason reason,
        CancellationToken ct = default)
    {
        var query = GetQueryableSet()
            .Include(r => r.DocumentFile)
            .Include(r => r.Comment)
            .Where(r => r.Status == ContentStatus.PendingReview && r.Reason == reason);

        if (documentFileId.HasValue)
            query = query.Where(r => r.DocumentFileId == documentFileId.Value);
        else if (commentId.HasValue)
            query = query.Where(r => r.CommentId == commentId.Value);
        else
            return [];

        return await query.OrderBy(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task<int> GetDailyInstantApproveCountAsync(Guid userId, DateTimeOffset today, CancellationToken ct = default)
    {
        var startOfDay = today.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await GetQueryableSet()
            .Where(r => r.CreatedById == userId
                && r.ReviewedById == userId
                && r.Status == ContentStatus.Approved
                && r.CreatedAt >= startOfDay
                && r.CreatedAt < endOfDay)
            .CountAsync(ct);
    }

    public async Task<Report?> GetUserPendingReportAsync(
        Guid userId,
        Guid? documentFileId,
        Guid? commentId,
        CancellationToken ct = default)
    {
        return await GetQueryableSet()
            .Where(r => r.CreatedById == userId
                && r.Status == ContentStatus.PendingReview
                && ((documentFileId.HasValue && r.DocumentFileId == documentFileId.Value)
                    || (commentId.HasValue && r.CommentId == commentId.Value)))
            .FirstOrDefaultAsync(ct);
    }
}
