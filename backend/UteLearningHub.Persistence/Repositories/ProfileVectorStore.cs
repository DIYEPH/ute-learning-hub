using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UteLearningHub.Persistence.Repositories;

public class ProfileVectorStore : IProfileVectorStore
{
    private readonly ApplicationDbContext _dbContext;

    public ProfileVectorStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<ProfileVector> Query() => _dbContext.Set<ProfileVector>().AsQueryable();

    public async Task<ProfileVector?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ProfileVector>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpsertAsync(ProfileVector vector, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Set<ProfileVector>()
            .AnyAsync(x => x.Id == vector.Id, cancellationToken);

        if (exists)
            _dbContext.Set<ProfileVector>().Update(vector);
        else
            await _dbContext.Set<ProfileVector>().AddAsync(vector, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}