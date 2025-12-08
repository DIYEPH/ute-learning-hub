using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class TagRepository : Repository<Tag, Guid>, ITagRepository
{
    public TagRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<IList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(t => ids.Contains(t.Id));
        if (!includeDeleted)
            query = query.Where(t => !t.IsDeleted);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Tag?> FindByNameAsync(string name, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLowerInvariant();
        var query = DbSet.Where(t => t.TagName != null);
        if (!includeDeleted)
            query = query.Where(t => !t.IsDeleted);
        return await query.FirstOrDefaultAsync(t => t.TagName!.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<int> GetDocumentCountAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Id == tagId)
            .SelectMany(t => t.DocumentTags)
            .CountAsync(cancellationToken);
    }
}
