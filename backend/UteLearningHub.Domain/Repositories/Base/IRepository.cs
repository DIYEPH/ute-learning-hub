using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Repositories.UnitOfWork;

namespace UteLearningHub.Domain.Repositories.Base;

public interface IRepository<TEntity, in TKey> : IConcurrencyHandler<TEntity>
    where TEntity : BaseEntity<TKey>, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
    IQueryable<TEntity> GetQueryableSet();
    Task<List<TEntity>> GetAllAsync(bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdAsync(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task ToPaginationAsync(ref IQueryable<TEntity> query, int page, int size);
    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, Guid? deletedById = null, CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
}
