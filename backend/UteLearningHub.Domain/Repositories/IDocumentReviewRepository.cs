using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IDocumentReviewRepository : IRepository<DocumentReview, Guid>
{
    Task<DocumentReview?> GetByIdAndUserIdAsync(Guid documentFileId, Guid userId, bool disableTracking = false, CancellationToken cancellationToken = default);
}
