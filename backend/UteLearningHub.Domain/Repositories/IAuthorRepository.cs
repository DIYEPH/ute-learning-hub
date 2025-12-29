using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IAuthorRepository : IRepository<Author, Guid>
{
    Task<IList<Author>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<Author?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
}

