using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos.Statistics;
using UteLearningHub.Application.Services.Statistics;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Statistics;

public class StatisticsService(ApplicationDbContext db, IDateTimeProvider dateTimeProvider) : IStatisticsService
{
    // Thống kê tổng quan
    public async Task<OverviewStatsDto> GetOverviewStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        return new OverviewStatsDto
        {
            TotalUsers = await db.Users.CountAsync(ct),
            NewUsersLast7Days = await db.Users.CountAsync(u => u.CreatedAt >= last7Days, ct),
            TotalDocuments = await db.Documents.CountAsync(ct),
            NewDocumentsLast7Days = await db.Documents.CountAsync(d => d.CreatedAt >= last7Days, ct),
            TotalViews = await db.DocumentFiles.SumAsync(df => (long)df.ViewCount, ct),
            PendingReports = await db.Reports.CountAsync(r => r.Status == ContentStatus.PendingReview, ct),
            PendingDocumentFiles = await db.DocumentFiles.CountAsync(df => df.Status == ContentStatus.PendingReview, ct),
            TotalConversations = await db.Conversations.CountAsync(ct),
            UsersOverTime = await GetTimeSeriesAsync(db.Users.Where(u => u.CreatedAt >= startDate), u => u.CreatedAt, days, ct),
            DocumentsOverTime = await GetTimeSeriesAsync(db.Documents.Where(d => d.CreatedAt >= startDate), d => d.CreatedAt, days, ct)
        };
    }

    // Thống kê tài liệu
    public async Task<DocumentStatsDto> GetDocumentStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var totalFiles = await db.DocumentFiles.CountAsync(ct);
        var totalViews = await db.DocumentFiles.SumAsync(df => (long)df.ViewCount, ct);

        return new DocumentStatsDto
        {
            TotalDocuments = totalFiles,
            ApprovedDocuments = await db.DocumentFiles.CountAsync(df => df.Status == ContentStatus.Approved, ct),
            PendingDocuments = await db.DocumentFiles.CountAsync(df => df.Status == ContentStatus.PendingReview, ct),
            TotalViews = totalViews,
            AvgViewsPerDocument = totalFiles > 0 ? (double)totalViews / totalFiles : 0,
            TotalUsefulReviews = await db.DocumentReviews.CountAsync(r => r.DocumentReviewType == DocumentReviewType.Useful, ct),
            TotalNotUsefulReviews = await db.DocumentReviews.CountAsync(r => r.DocumentReviewType == DocumentReviewType.NotUseful, ct),
            DocumentsBySubject = await db.DocumentFiles
                .Where(df => df.Document.SubjectId != null)
                .GroupBy(df => df.Document.Subject!.SubjectName)
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).Take(10).ToListAsync(ct),
            DocumentsByType = await db.DocumentFiles
                .GroupBy(df => df.Document.Type.TypeName)
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).ToListAsync(ct),
            TopDocumentsByViews = await db.DocumentFiles
                .Include(df => df.Document)
                .OrderByDescending(df => df.ViewCount).Take(10)
                .Select(df => new TopDocumentDto { Id = df.DocumentId, Name = df.Document.DocumentName, ViewCount = df.ViewCount })
                .ToListAsync(ct)
        };
    }

    // Thống kê người dùng
    public async Task<UserStatsDto> GetUserStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        var totalUsers = await db.Users.CountAsync(ct);

        return new UserStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersLast7Days = await db.Users.CountAsync(u => u.LastLoginAt >= last7Days, ct),
            BannedUsers = await db.Users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > now, ct),
            AvgTrustScore = totalUsers > 0 ? await db.Users.AverageAsync(u => (double)u.TrustScore, ct) : 0,
            UsersByMajor = await db.Users.Where(u => u.MajorId != null)
                .GroupBy(u => u.Major!.MajorName)
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).Take(10).ToListAsync(ct),
            UsersByTrustLevel = await db.Users
                .GroupBy(u => u.TrustLever)
                .Select(g => new ChartDataItem { Label = g.Key.ToString(), Value = g.Count() })
                .ToListAsync(ct),
            TopContributors = await db.Documents
                .GroupBy(d => d.CreatedById)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Join(db.Users, x => x.UserId, u => u.Id, (x, u) => new TopUserDto
                {
                    Id = u.Id, FullName = u.FullName, AvatarUrl = u.AvatarUrl,
                    DocumentCount = x.Count, TrustScore = u.TrustScore
                }).ToListAsync(ct),
            RegistrationsOverTime = await GetTimeSeriesAsync(db.Users.Where(u => u.CreatedAt >= startDate), u => u.CreatedAt, days, ct)
        };
    }

    // Thống kê kiểm duyệt
    public async Task<ModerationStatsDto> GetModerationStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);

        return new ModerationStatsDto
        {
            TotalReports = await db.Reports.CountAsync(ct),
            PendingReports = await db.Reports.CountAsync(r => r.Status == ContentStatus.PendingReview, ct),
            ApprovedReports = await db.Reports.CountAsync(r => r.Status == ContentStatus.Approved, ct),
            PendingComments = await db.Comments.CountAsync(c => c.Status == ContentStatus.PendingReview, ct),
            HiddenComments = await db.Comments.CountAsync(c => c.Status == ContentStatus.Hidden, ct),
            PendingDocumentFiles = await db.DocumentFiles.CountAsync(df => df.Status == ContentStatus.PendingReview, ct),
            ReportsByReason = await db.Reports
                .GroupBy(r => r.Reason)
                .Select(g => new ChartDataItem { Label = g.Key.ToString(), Value = g.Count() })
                .ToListAsync(ct),
            ReportsOverTime = await GetTimeSeriesAsync(db.Reports.Where(r => r.CreatedAt >= startDate), r => r.CreatedAt, days, ct),
            TopReportedUsers = await db.Reports.Where(r => r.DocumentFileId != null)
                .Select(r => r.DocumentFile!.CreatedById)
                .GroupBy(id => id)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10)
                .Join(db.Users, x => x.UserId, u => u.Id, (x, u) => new TopReportedUserDto
                {
                    Id = u.Id, FullName = u.FullName, AvatarUrl = u.AvatarUrl, ReportCount = x.Count
                }).ToListAsync(ct)
        };
    }

    // Thống kê cuộc trò chuyện
    public async Task<ConversationStatsDto> GetConversationStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        var totalConv = await db.Conversations.CountAsync(ct);

        return new ConversationStatsDto
        {
            TotalConversations = totalConv,
            ActiveConversations = await db.Conversations.CountAsync(c => c.ConversationStatus == ConversationStatus.Active, ct),
            TotalMessagesLast7Days = await db.Messages.CountAsync(m => m.CreatedAt >= last7Days, ct),
            AvgMembersPerConversation = totalConv > 0
                ? await db.ConversationMembers.GroupBy(cm => cm.ConversationId).Select(g => g.Count()).DefaultIfEmpty().AverageAsync(ct)
                : 0,
            ConversationsBySubject = await db.Conversations.Where(c => c.SubjectId != null)
                .GroupBy(c => c.Subject.SubjectName)
                .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
                .OrderByDescending(x => x.Value).Take(10).ToListAsync(ct),
            MessagesOverTime = await GetTimeSeriesAsync(db.Messages.Where(m => m.CreatedAt >= startDate), m => m.CreatedAt, days, ct)
        };
    }

    // Helper: Tạo time series data
    private async Task<List<TimeSeriesDataPoint>> GetTimeSeriesAsync<T>(
        IQueryable<T> query, Func<T, DateTimeOffset> dateSelector, int days, CancellationToken ct) where T : class
    {
        var now = dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days).Date;

        var allDates = Enumerable.Range(0, days + 1)
            .Select(i => DateOnly.FromDateTime(startDate.AddDays(i)))
            .ToList();

        var items = await query.ToListAsync(ct);
        var dataDict = items
            .GroupBy(x => DateOnly.FromDateTime(dateSelector(x).Date))
            .ToDictionary(g => g.Key, g => (long)g.Count());

        return allDates.Select(date => new TimeSeriesDataPoint
        {
            Date = date,
            Value = dataDict.GetValueOrDefault(date, 0)
        }).ToList();
    }
}