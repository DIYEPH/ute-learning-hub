using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<IList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<Tag?> FindByNameAsync(string name, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<int> GetDocumentCountAsync(Guid tagId, CancellationToken cancellationToken = default);
}
