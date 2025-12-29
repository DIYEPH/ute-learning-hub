using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Repositories.UnitOfWork;

namespace UteLearningHub.Domain.Repositories.Base;

public interface IRepository<TEntity, in TKey> : IConcurrencyHandler<TEntity>
    where TEntity : BaseEntity<TKey>, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    IQueryable<TEntity> GetQueryableSet();
    Task<TEntity?> GetByIdAsync(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default);
    void Add(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
}
