using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IReportRepository : IRepository<Report, Guid>
{
    Task<Report?> GetByIdWithContentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IList<Report>> GetRelatedPendingReportsAsync(
        Guid? documentFileId,
        Guid? commentId,
        CancellationToken cancellationToken = default);

    Task<int> GetDailyInstantApproveCountAsync(Guid userId, DateTimeOffset today, CancellationToken cancellationToken = default);
}
