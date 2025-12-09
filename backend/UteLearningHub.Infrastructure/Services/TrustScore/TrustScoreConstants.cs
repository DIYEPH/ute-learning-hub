using UteLearningHub.Application.Services.TrustScore;

namespace UteLearningHub.Infrastructure.Services.TrustScore;

public static class TrustScoreConstants
{

    /// Điểm thưởng/phạt cho từng hành động
    public static readonly Dictionary<string, int> ActionPoints = new()
    {
        { "CreateDocument", 5 },           // +5 điểm khi đăng bài
        { "DocumentLiked", 2 },           // +2 điểm khi được like
        { "DocumentUnliked", -2 },        // -2 điểm khi bị unlike
        { "ReportApproved", 3 }           // +3 điểm khi report được approve
    };

    /// Lấy điểm thưởng/phạt cho một hành động
    public static int GetActionPoints(string actionName)
    {
        return ActionPoints.GetValueOrDefault(actionName, 0);
    }
}

