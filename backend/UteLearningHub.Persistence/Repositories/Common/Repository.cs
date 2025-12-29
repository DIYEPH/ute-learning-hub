using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Repositories.Base;
using UteLearningHub.Domain.Repositories.UnitOfWork;

namespace UteLearningHub.Persistence.Repositories.Common;

public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : BaseEntity<TKey>, IAggregateRoot
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly IDateTimeProvider _dateTimeProvider;
    public Repository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext;
        _dateTimeProvider = dateTimeProvider;
    }

    protected DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();
    public IUnitOfWork UnitOfWork => _dbContext;
    public IQueryable<TEntity> GetQueryableSet()
    {
        return DbSet;
    }
    public async Task<TEntity?> GetByIdAsync(TKey id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = DbSet;

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public void Add(TEntity entity)
    {
        entity.CreatedAt = _dateTimeProvider.OffsetNow;
        DbSet.Add(entity);
    }
    public void Update(TEntity entity)
    {
        entity.UpdatedAt = _dateTimeProvider.OffsetNow;
        DbSet.Update(entity);
    }
    public void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }
    public async Task BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.OffsetNow;
        var list = entities.ToList();

        foreach (var entity in list)
            entity.CreatedAt = now;

        await _dbContext.BulkInsertAsync(list, cancellationToken: cancellationToken);
    }
    public async Task BulkUpdateAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.OffsetNow;
        var list = entities.ToList();

        foreach (var entity in list)
            entity.UpdatedAt = now;

        await _dbContext.BulkUpdateAsync(list, cancellationToken: cancellationToken);
    }

    public async Task BulkDeleteAsync(List<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.BulkDeleteAsync(entities, cancellationToken: cancellationToken);
    }

    public bool IsDbUpdateConcurrencyException(Exception ex) => ex is DbUpdateConcurrencyException;

    public void SetRowVersion(TEntity entity, byte[] version)
    {
        _dbContext.Entry(entity).OriginalValues[nameof(entity.RowVersion)] = version;
    }
}
