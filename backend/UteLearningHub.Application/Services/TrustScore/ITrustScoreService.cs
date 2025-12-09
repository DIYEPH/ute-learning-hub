namespace UteLearningHub.Application.Services.TrustScore;

public interface ITrustScoreService
{
    Task AddTrustScoreAsync(Guid userId, int points, string reason, CancellationToken cancellationToken = default);
    Task<int> GetTrustScoreAsync(Guid userId, CancellationToken cancellationToken = default);
}

public enum ActionPoints
{
    CreateDocument,
    DocumentLiked,
    ReportApproved,
}
