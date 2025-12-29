namespace UteLearningHub.Domain.Policies;

public static class TrustScorePolicy
{
    // Điểm thưởng/phạt cho từng hành động
    public static readonly Dictionary<string, int> ActionPointValues = new()
    {
        { "CreateDocumentFile", 5 },           // +5 điểm khi đăng bài
        { "DocumentLiked", 2 },             // +2 điểm khi được like
        { "DocumentUnliked", -2 },          // -2 điểm khi bị unlike
        { "ReportApproved", 3 }             // +3 điểm khi report được approve 
    };
    // Report reward configuration
    public const int MaxRewardedReporters = 10;  // Chỉ top 10 người đầu tiên được thưởng
    public const int TrustedMemberDailyReportLimit = 2;  // TrustedMember trở lên chỉ được báo cáo + duyệt tự động 3 lần/ngày, từ lần thứ 4 sẽ vào pending như người bình thường (cần admin duyệt, đúng thì mới có thưởng)

    // Time-based reward tiers (từ lúc content được tạo)
    public const int ReportWithin24Hours = 4;   // +4 điểm nếu báo cáo trong 24h
    public const int ReportWithin72Hours = 2;   // +2 điểm nếu báo cáo trong 24-72h
    public const int ReportAfter72Hours = 1;    // +1 điểm nếu báo cáo sau 72h

    public static int GetActionPoints(string actionName)
        => ActionPointValues.GetValueOrDefault(actionName, 0);

    /// Tính điểm thưởng dựa trên thời gian báo cáo so với thời gian tạo content
    public static int CalculateReportRewardPoints(DateTimeOffset contentCreatedAt, DateTimeOffset reportCreatedAt)
    {
        var timeDiff = reportCreatedAt - contentCreatedAt;

        return timeDiff.TotalHours switch
        {
            <= 24 => ReportWithin24Hours,
            <= 72 => ReportWithin72Hours,
            _ => ReportAfter72Hours
        };
    }
}

