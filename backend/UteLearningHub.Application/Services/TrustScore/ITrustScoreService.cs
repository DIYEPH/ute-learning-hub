namespace UteLearningHub.Application.Services.TrustScore;

public interface ITrustScoreService
{
    Task AddTrustScoreAsync(Guid userId, int points, string reason, Guid? entityId = null, CancellationToken cancellationToken = default);
    Task RevertTrustScoreByEntityAsync(Guid entityId, CancellationToken cancellationToken = default);
    Task<int> GetTrustScoreAsync(Guid userId, CancellationToken cancellationToken = default);
}

public enum ActionPoints
{
    CreateDocument,
    DocumentLiked,
    ReportApproved,
}
