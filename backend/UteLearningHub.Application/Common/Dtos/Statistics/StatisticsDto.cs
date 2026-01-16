namespace UteLearningHub.Application.Common.Dtos.Statistics;

// Thống kê tổng quan
public class OverviewStatsDto
{
    public int TotalUsers { get; set; }
    public int NewUsersLast7Days { get; set; }
    public int TotalDocuments { get; set; }
    public int NewDocumentsLast7Days { get; set; }
    public long TotalViews { get; set; }
    public int PendingReports { get; set; }
    public int PendingDocumentFiles { get; set; }
    public int TotalConversations { get; set; }
    public List<TimeSeriesDataPoint> UsersOverTime { get; set; } = [];
    public List<TimeSeriesDataPoint> DocumentsOverTime { get; set; } = [];
}

// Thống kê tài liệu
public class DocumentStatsDto
{
    public int TotalDocuments { get; set; }
    public int ApprovedDocuments { get; set; }
    public int PendingDocuments { get; set; }
    public long TotalViews { get; set; }
    public double AvgViewsPerDocument { get; set; }
    public int TotalUsefulReviews { get; set; }
    public int TotalNotUsefulReviews { get; set; }
    public List<ChartDataItem> DocumentsBySubject { get; set; } = [];
    public List<ChartDataItem> DocumentsByType { get; set; } = [];
    public List<TopDocumentDto> TopDocumentsByViews { get; set; } = [];
}

// Thống kê người dùng
public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsersLast7Days { get; set; }
    public int BannedUsers { get; set; }
    public double AvgTrustScore { get; set; }
    public List<ChartDataItem> UsersByMajor { get; set; } = [];
    public List<ChartDataItem> UsersByTrustLevel { get; set; } = [];
    public List<TopUserDto> TopContributors { get; set; } = [];
    public List<TimeSeriesDataPoint> RegistrationsOverTime { get; set; } = [];
}

// Thống kê kiểm duyệt
public class ModerationStatsDto
{
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }
    public int ApprovedReports { get; set; }
    public int PendingComments { get; set; }
    public int HiddenComments { get; set; }
    public int PendingDocumentFiles { get; set; }
    public List<ChartDataItem> ReportsByReason { get; set; } = [];
    public List<TimeSeriesDataPoint> ReportsOverTime { get; set; } = [];
    public List<TopReportedUserDto> TopReportedUsers { get; set; } = [];
}

// Thống kê cuộc trò chuyện
public class ConversationStatsDto
{
    public int TotalConversations { get; set; }
    public int ActiveConversations { get; set; }
    public int TotalMessagesLast7Days { get; set; }
    public double AvgMembersPerConversation { get; set; }
    public List<ChartDataItem> ConversationsBySubject { get; set; } = [];
    public List<TimeSeriesDataPoint> MessagesOverTime { get; set; } = [];
}

// Supporting DTOs
public class TimeSeriesDataPoint
{
    public DateOnly Date { get; set; }
    public long Value { get; set; }
}

public class ChartDataItem
{
    public string Label { get; set; } = string.Empty;
    public long Value { get; set; }
}

public class TopDocumentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}

public class TopUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int DocumentCount { get; set; }
    public int TrustScore { get; set; }
}

public class TopReportedUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int ReportCount { get; set; }
}