using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using DomainType = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Persistence.Repositories;

public class TypeRepository : Repository<DomainType, Guid>, ITypeRepository
{
    public TypeRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public async Task<DomainType?> FindByNameAsync(string name, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToLowerInvariant();
        var query = DbSet.AsQueryable();
        if (!includeDeleted)
            query = query.Where(t => !t.IsDeleted);
        return await query.FirstOrDefaultAsync(t => t.TypeName.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<int> GetDocumentCountAsync(Guid typeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Id == typeId)
            .SelectMany(t => t.Documents.Where(d => !d.IsDeleted))
            .CountAsync(cancellationToken);
    }
}
