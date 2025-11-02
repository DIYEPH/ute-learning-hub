using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IBaseRepository<TEntity, in TKey> : IConcurrencyHandler<TEntity>
    where TEntity : BaseEntity<TKey>, IAggregateRoot
{
    IQueryable<TEntity> GetQueryableSet();
    Task<List<TEntity>> GetAllAsync(bool disableTracking = false, CancellationToken cancellationToken = default);
    Task<TEntity> GetById(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task ToPaginationAsync(ref IQueryable<TEntity> query, int page, int size);
    Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
    Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default);
}
