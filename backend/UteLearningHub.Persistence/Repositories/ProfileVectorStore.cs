using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Persistence.Repositories;

public class ProfileVectorStore : IProfileVectorStore
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public ProfileVectorStore(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IQueryable<ProfileVector> Query()
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.Set<ProfileVector>().AsQueryable();
    }

    public async Task<ProfileVector?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<ProfileVector>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpsertAsync(ProfileVector vector, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Find existing vector by UserId (not Id) to properly update
        var existingVector = await dbContext.Set<ProfileVector>()
            .FirstOrDefaultAsync(x => x.UserId == vector.UserId && x.IsActive, cancellationToken);

        if (existingVector != null)
        {
            // Update existing vector
            existingVector.EmbeddingJson = vector.EmbeddingJson;
            existingVector.CalculatedAt = vector.CalculatedAt;
            dbContext.Set<ProfileVector>().Update(existingVector);
        }
        else
        {
            // Add new vector
            await dbContext.Set<ProfileVector>().AddAsync(vector, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}