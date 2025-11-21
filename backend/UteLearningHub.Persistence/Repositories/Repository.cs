using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Persistence.Repositories;

public class Repository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>, IAggregateRoot
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    public Repository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();
    public IUnitOfWork UnitOfWork => _dbContext;
    public IQueryable<TEntity> GetQueryableSet()
    {
        return _dbContext.Set<TEntity>();
    }
    public async Task<TEntity?> GetByIdAsync(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => EqualityComparer<TKey>.Default.Equals(e.Id, id), cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = _dateTimeProvider.OffsetNow;
        await DbSet.AddAsync(entity, cancellationToken);
    }
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = _dateTimeProvider.OffsetNow;
        DbSet.Update(entity);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
    public async Task AddOrUpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (EqualityComparer<TKey>.Default.Equals(entity.Id, default!))
            await AddAsync(entity, cancellationToken);
        else
            await UpdateAsync(entity, cancellationToken);
    }
    public async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.OffsetNow;
        var list = entities.ToList();
        foreach (var entity in list)
            entity.CreatedAt = now;
        
        await _dbContext.BulkInsertAsync(entities, cancellationToken: cancellationToken);
    }
    public async Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.OffsetNow;
        var list = entities.ToList();
        foreach (var entity in list)
            entity.UpdatedAt = now;
        await _dbContext.BulkUpdateAsync(entities, cancellationToken: cancellationToken);
    }

    public async Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.BulkDeleteAsync(entities, cancellationToken: cancellationToken);
    }


    public bool IsDbUpdateConcurrencyException(Exception ex)
    {
        return ex is DbUpdateConcurrencyException;
    }

    public void SetRowVersion(TEntity entity, byte[] version)
    {
        _dbContext.Entry(entity).OriginalValues[nameof(entity.RowVersion)] = version;
    }

    public Task ToPaginationAsync(ref IQueryable<TEntity> query, int page, int size)
    {
        throw new NotImplementedException();
    }
}
