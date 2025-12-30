using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos.Statistics;
using UteLearningHub.Application.Services.Statistics;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Persistence;

namespace UteLearningHub.Infrastructure.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StatisticsService(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<OverviewStatsDto> GetOverviewStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        var totalUsers = await _context.Users.CountAsync(u => !u.IsDeleted, ct);
        var newUsersLast7Days = await _context.Users.CountAsync(u => !u.IsDeleted && u.CreatedAt >= last7Days, ct);
        
        var totalDocuments = await _context.Documents.CountAsync(d => !d.IsDeleted, ct);
        var newDocumentsLast7Days = await _context.Documents.CountAsync(d => !d.IsDeleted && d.CreatedAt >= last7Days, ct);
        
        var totalViews = await _context.DocumentFiles.Where(df => !df.IsDeleted).SumAsync(df => (long)df.ViewCount, ct);
        
        var pendingReports = await _context.Reports.CountAsync(r => !r.IsDeleted && r.Status == ContentStatus.PendingReview, ct);
        var pendingDocumentFiles = await _context.DocumentFiles.CountAsync(df => !df.IsDeleted && df.Status == ContentStatus.PendingReview, ct);
        
        var totalConversations = await _context.Conversations.CountAsync(c => !c.IsDeleted, ct);

        // Time series data
        var usersOverTime = await GetTimeSeriesAsync(
            _context.Users.Where(u => !u.IsDeleted && u.CreatedAt >= startDate),
            u => u.CreatedAt, days, ct);
            
        var documentsOverTime = await GetTimeSeriesAsync(
            _context.Documents.Where(d => !d.IsDeleted && d.CreatedAt >= startDate),
            d => d.CreatedAt, days, ct);

        return new OverviewStatsDto
        {
            TotalUsers = totalUsers,
            NewUsersLast7Days = newUsersLast7Days,
            TotalDocuments = totalDocuments,
            NewDocumentsLast7Days = newDocumentsLast7Days,
            TotalViews = totalViews,
            PendingReports = pendingReports,
            PendingDocumentFiles = pendingDocumentFiles,
            TotalConversations = totalConversations,
            UsersOverTime = usersOverTime,
            DocumentsOverTime = documentsOverTime
        };
    }

    public async Task<DocumentStatsDto> GetDocumentStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);

        var documents = _context.Documents.Where(d => !d.IsDeleted);
        var documentFiles = _context.DocumentFiles.Where(df => !df.IsDeleted);

        var totalDocuments = await documents.CountAsync(ct);
        var approvedDocuments = await documentFiles.CountAsync(df => df.Status == ContentStatus.Approved, ct);
        var pendingDocuments = await documentFiles.CountAsync(df => df.Status == ContentStatus.PendingReview, ct);
        var totalViews = await documentFiles.SumAsync(df => (long)df.ViewCount, ct);
        var avgViews = totalDocuments > 0 ? (double)totalViews / totalDocuments : 0;

        var reviews = _context.DocumentReviews;
        var usefulReviews = await reviews.CountAsync(r => r.DocumentReviewType == DocumentReviewType.Useful, ct);
        var notUsefulReviews = await reviews.CountAsync(r => r.DocumentReviewType == DocumentReviewType.NotUseful, ct);

        // Documents by Subject
        var docsBySubject = await documents
            .Where(d => d.SubjectId != null)
            .GroupBy(d => d.Subject!.SubjectName)
            .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        // Documents by Type
        var docsByType = await documents
            .GroupBy(d => d.Type.TypeName)
            .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .ToListAsync(ct);

        // Top documents by views
        var topDocs = await documentFiles
            .Include(df => df.Document)
            .OrderByDescending(df => df.ViewCount)
            .Take(10)
            .Select(df => new TopDocumentDto
            {
                Id = df.DocumentId,
                Name = df.Document.DocumentName,
                ViewCount = df.ViewCount
            })
            .ToListAsync(ct);

        return new DocumentStatsDto
        {
            TotalDocuments = totalDocuments,
            ApprovedDocuments = approvedDocuments,
            PendingDocuments = pendingDocuments,
            TotalViews = totalViews,
            AvgViewsPerDocument = avgViews,
            TotalUsefulReviews = usefulReviews,
            TotalNotUsefulReviews = notUsefulReviews,
            DocumentsBySubject = docsBySubject,
            DocumentsByType = docsByType,
            TopDocumentsByViews = topDocs
        };
    }

    public async Task<UserStatsDto> GetUserStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        var users = _context.Users.Where(u => !u.IsDeleted);

        var totalUsers = await users.CountAsync(ct);
        var activeUsers = await users.CountAsync(u => u.LastLoginAt >= last7Days, ct);
        var bannedUsers = await users.CountAsync(u => u.LockoutEnd != null && u.LockoutEnd > now, ct);
        var avgTrustScore = totalUsers > 0 ? await users.AverageAsync(u => (double)u.TrustScore, ct) : 0;

        // Users by Major
        var usersByMajor = await users
            .Where(u => u.MajorId != null)
            .GroupBy(u => u.Major!.MajorName)
            .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        // Users by TrustLevel
        var usersByTrust = await users
            .GroupBy(u => u.TrustLever)
            .Select(g => new ChartDataItem { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(ct);

        // Top contributors (by document count)
        var topContributors = await _context.Documents
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.CreatedById)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .Join(_context.Users, x => x.UserId, u => u.Id, (x, u) => new TopUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                DocumentCount = x.Count,
                TrustScore = u.TrustScore
            })
            .ToListAsync(ct);

        // Registrations over time
        var registrations = await GetTimeSeriesAsync(
            users.Where(u => u.CreatedAt >= startDate),
            u => u.CreatedAt, days, ct);

        return new UserStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsersLast7Days = activeUsers,
            BannedUsers = bannedUsers,
            AvgTrustScore = avgTrustScore,
            UsersByMajor = usersByMajor,
            UsersByTrustLevel = usersByTrust,
            TopContributors = topContributors,
            RegistrationsOverTime = registrations
        };
    }

    public async Task<ModerationStatsDto> GetModerationStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);

        var reports = _context.Reports.Where(r => !r.IsDeleted);
        var comments = _context.Comments.Where(c => !c.IsDeleted);
        var documentFiles = _context.DocumentFiles.Where(df => !df.IsDeleted);

        var totalReports = await reports.CountAsync(ct);
        var pendingReports = await reports.CountAsync(r => r.Status == ContentStatus.PendingReview, ct);
        var approvedReports = await reports.CountAsync(r => r.Status == ContentStatus.Approved, ct);
        
        var pendingComments = await comments.CountAsync(c => c.Status == ContentStatus.PendingReview, ct);
        var hiddenComments = await comments.CountAsync(c => c.Status == ContentStatus.Hidden, ct);
        var pendingDocFiles = await documentFiles.CountAsync(df => df.Status == ContentStatus.PendingReview, ct);

        // Reports by Reason
        var reportsByReason = await reports
            .GroupBy(r => r.Reason)
            .Select(g => new ChartDataItem { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(ct);

        // Reports over time
        var reportsOverTime = await GetTimeSeriesAsync(
            reports.Where(r => r.CreatedAt >= startDate),
            r => r.CreatedAt, days, ct);

        // Top reported users (by documents/comments they created that got reported)
        var topReported = await reports
            .Where(r => r.DocumentFileId != null)
            .Select(r => r.DocumentFile!.CreatedById)
            .GroupBy(id => id)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .Join(_context.Users, x => x.UserId, u => u.Id, (x, u) => new TopReportedUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                AvatarUrl = u.AvatarUrl,
                ReportCount = x.Count
            })
            .ToListAsync(ct);

        return new ModerationStatsDto
        {
            TotalReports = totalReports,
            PendingReports = pendingReports,
            ApprovedReports = approvedReports,
            PendingComments = pendingComments,
            HiddenComments = hiddenComments,
            PendingDocumentFiles = pendingDocFiles,
            ReportsByReason = reportsByReason,
            ReportsOverTime = reportsOverTime,
            TopReportedUsers = topReported
        };
    }

    public async Task<ConversationStatsDto> GetConversationStatsAsync(int days = 30, CancellationToken ct = default)
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days);
        var last7Days = now.AddDays(-7);

        var conversations = _context.Conversations.Where(c => !c.IsDeleted);
        var messages = _context.Messages.Where(m => !m.IsDeleted);

        var totalConversations = await conversations.CountAsync(ct);
        var activeConversations = await conversations.CountAsync(c => c.ConversationStatus == ConversationStatus.Active, ct);
        var messagesLast7Days = await messages.CountAsync(m => m.CreatedAt >= last7Days, ct);
        
        var avgMembers = totalConversations > 0 
            ? await _context.ConversationMembers
                .GroupBy(cm => cm.ConversationId)
                .Select(g => g.Count())
                .DefaultIfEmpty()
                .AverageAsync(ct)
            : 0;

        // Conversations by Subject
        var convBySubject = await conversations
            .Where(c => c.SubjectId != null)
            .GroupBy(c => c.Subject.SubjectName)
            .Select(g => new ChartDataItem { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToListAsync(ct);

        // Messages over time
        var messagesOverTime = await GetTimeSeriesAsync(
            messages.Where(m => m.CreatedAt >= startDate),
            m => m.CreatedAt, days, ct);

        return new ConversationStatsDto
        {
            TotalConversations = totalConversations,
            ActiveConversations = activeConversations,
            TotalMessagesLast7Days = messagesLast7Days,
            AvgMembersPerConversation = avgMembers,
            ConversationsBySubject = convBySubject,
            MessagesOverTime = messagesOverTime
        };
    }

    private async Task<List<TimeSeriesDataPoint>> GetTimeSeriesAsync<T>(
        IQueryable<T> query,
        Func<T, DateTimeOffset> dateSelector,
        int days,
        CancellationToken ct) where T : class
    {
        var now = _dateTimeProvider.OffsetUtcNow;
        var startDate = now.AddDays(-days).Date;

        // Get all dates in range
        var allDates = Enumerable.Range(0, days + 1)
            .Select(i => DateOnly.FromDateTime(startDate.AddDays(i)))
            .ToList();

        // First materialize the query, then group in memory
        var items = await query.ToListAsync(ct);
        
        var dataDict = items
            .GroupBy(x => DateOnly.FromDateTime(dateSelector(x).Date))
            .ToDictionary(g => g.Key, g => (long)g.Count());

        // Fill in missing dates with 0
        return allDates.Select(date => new TimeSeriesDataPoint
        {
            Date = date,
            Value = dataDict.TryGetValue(date, out var value) ? value : 0
        }).ToList();
    }
}
