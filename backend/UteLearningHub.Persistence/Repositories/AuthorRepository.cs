using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class AuthorRepository : Repository<Author, Guid>, IAuthorRepository
{
    public AuthorRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<IList<Author>> GetByIdsAsync(IEnumerable<Guid> ids, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(a => ids.Contains(a.Id));
        if (!includeDeleted)
            query = query.Where(a => !a.IsDeleted);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Author?> FindByNameAsync(string name, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLowerInvariant();
        var query = DbSet.AsQueryable();
        if (!includeDeleted)
            query = query.Where(a => !a.IsDeleted);
        return await query.FirstOrDefaultAsync(a => a.FullName.ToLower() == normalizedName, cancellationToken);
    }
}
