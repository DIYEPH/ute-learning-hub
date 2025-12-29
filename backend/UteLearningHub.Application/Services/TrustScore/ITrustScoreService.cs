using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.TrustScore;

public interface ITrustScoreService
{
    Task AddTrustScoreAsync(Guid userId, int points, string reason, Guid? entityId = null, TrustEntityType? entityType = null, CancellationToken cancellationToken = default);
    Task RevertTrustScoreByEntityAsync(Guid entityId, TrustEntityType? entityType = null, CancellationToken cancellationToken = default);
}
