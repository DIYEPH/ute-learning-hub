using UteLearningHub.Application.Common.Dtos.Statistics;

namespace UteLearningHub.Application.Services.Statistics;

public interface IStatisticsService
{
    /// <summary>
    /// Get overview dashboard statistics
    /// </summary>
    Task<OverviewStatsDto> GetOverviewStatsAsync(int days = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Get document statistics
    /// </summary>
    Task<DocumentStatsDto> GetDocumentStatsAsync(int days = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Get user statistics
    /// </summary>
    Task<UserStatsDto> GetUserStatsAsync(int days = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Get moderation/reports statistics
    /// </summary>
    Task<ModerationStatsDto> GetModerationStatsAsync(int days = 30, CancellationToken ct = default);
    
    /// <summary>
    /// Get conversation statistics
    /// </summary>
    Task<ConversationStatsDto> GetConversationStatsAsync(int days = 30, CancellationToken ct = default);
}
