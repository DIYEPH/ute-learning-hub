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

    public async Task<Author?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLowerInvariant();
        var query = DbSet.AsQueryable();
        return await query.FirstOrDefaultAsync(a => a.FullName.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<IList<Author>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(t => ids.Contains(t.Id));
        return await query.ToListAsync(cancellationToken);
    }
}
