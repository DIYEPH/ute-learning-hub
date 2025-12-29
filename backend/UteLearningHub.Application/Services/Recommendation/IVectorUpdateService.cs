namespace UteLearningHub.Application.Services.Recommendation;

/// <summary>
/// Service để cập nhật vectors khi có thay đổi từ user actions
/// </summary>
public interface IVectorMaintenanceService
{
    /// <summary>
    /// Cập nhật user vector khi user thay đổi profile (major, subjects, tags)
    /// </summary>
    Task UpdateUserVectorAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật conversation vector khi conversation được tạo hoặc cập nhật
    /// </summary>
    Task UpdateConversationVectorAsync(Guid conversationId, CancellationToken cancellationToken = default);
}

