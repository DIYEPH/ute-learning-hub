namespace UteLearningHub.Application.Services.Recommendation;

public interface IVectorMaintenanceService
{
    Task UpdateUserVectorAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateConversationVectorAsync(Guid conversationId, CancellationToken cancellationToken = default);
}

