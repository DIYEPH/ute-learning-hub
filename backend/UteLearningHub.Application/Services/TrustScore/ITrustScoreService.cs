namespace UteLearningHub.Application.Services.TrustScore;

public interface ITrustScoreService
{
    Task AddTrustScoreAsync(Guid userId, int points, string reason, CancellationToken cancellationToken = default);
    Task<bool> CanPerformActionAsync(Guid userId, TrustScoreAction action, CancellationToken cancellationToken = default);
    Task<int> GetTrustScoreAsync(Guid userId, CancellationToken cancellationToken = default);
}

public enum TrustScoreAction
{
    CreateDocument,      // Đăng bài
    CreateReport,        // Báo cáo
    CreateComment,      // Bình luận
    CreateConversation  // Tạo nhóm học
}

