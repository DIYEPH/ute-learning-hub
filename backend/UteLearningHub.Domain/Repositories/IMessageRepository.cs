using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IMessageRepository : IRepository<Message, Guid>
{
    Task<Message?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    IQueryable<Message> GetQueryableWithDetails();
}