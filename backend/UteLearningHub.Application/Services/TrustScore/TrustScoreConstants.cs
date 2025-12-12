namespace UteLearningHub.Application.Services.TrustScore;

public static class TrustScoreConstants
{
    // Điểm thưởng/phạt cho từng hành động
    public static readonly Dictionary<string, int> ActionPointValues = new()
    {
        { "CreateDocument", 5 },           // +5 điểm khi đăng bài
        { "DocumentLiked", 2 },             // +2 điểm khi được like
        { "DocumentUnliked", -2 },          // -2 điểm khi bị unlike
        { "ReportApproved", 3 }             // +3 điểm khi report được approve (base)
    };

    // Report reward configuration
    public const int MaxRewardedReporters = 5;  // Chỉ top 5 người đầu tiên được thưởng
    public const int TrustedMemberDailyReportLimit = 3;  // TrustedMember chỉ được báo cáo + duyệt 3 lần/ngày

    // Time-based reward tiers (từ lúc content được tạo)
    public const int ReportWithin24Hours = 3;   // +3 điểm nếu báo cáo trong 24h
    public const int ReportWithin72Hours = 2;   // +2 điểm nếu báo cáo trong 24-72h
    public const int ReportAfter72Hours = 1;    // +1 điểm nếu báo cáo sau 72h

    public static int GetActionPoints(string actionName)
        => ActionPointValues.GetValueOrDefault(actionName, 0);

    /// <summary>
    /// Tính điểm thưởng dựa trên thời gian báo cáo so với thời gian tạo content
    /// </summary>
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

