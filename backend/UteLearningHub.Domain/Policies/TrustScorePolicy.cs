namespace UteLearningHub.Domain.Policies;

public static class TrustScorePolicy
{
    // Điểm thưởng/phạt cho từng hành động
    public static readonly Dictionary<string, int> ActionPointValues = new()
    {
        { "CreateDocumentFile", 5 },  // +5 đăng bài
        { "DocumentLiked", 2 },       // +2 được like
        { "DocumentUnliked", -2 }     // -2 bị unlike
    };

    // Report reward
    public const int MaxRewardedReporters = 10;           // Top 10 người đầu được thưởng
    public const int TrustedMemberDailyReportLimit = 2;   // Giới hạn auto-approve/ngày

    // Time-based reward tiers
    public const int ReportWithin24Hours = 4;   // Báo cáo trong 24h
    public const int ReportWithin72Hours = 2;   // Báo cáo trong 24-72h
    public const int ReportAfter72Hours = 1;    // Báo cáo sau 72h

    public static int GetActionPoints(string actionName)
        => ActionPointValues.GetValueOrDefault(actionName, 0);

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