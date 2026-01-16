using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Persistence.Repositories;

public class ProfileVectorStore(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IProfileVectorStore
{
    public IQueryable<ProfileVector> Query()
    {
        var dbContext = dbContextFactory.CreateDbContext();
        return dbContext.Set<ProfileVector>().AsQueryable();
    }

    public async Task UpsertAsync(ProfileVector vector, CancellationToken ct = default)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync(ct);
        var existing = await db.Set<ProfileVector>()
            .FirstOrDefaultAsync(x => x.UserId == vector.UserId && x.IsActive, ct);

        if (existing != null)
        {
            existing.EmbeddingJson = vector.EmbeddingJson;
            existing.CalculatedAt = vector.CalculatedAt;
            db.Set<ProfileVector>().Update(existing);
        }
        else
        {
            await db.Set<ProfileVector>().AddAsync(vector, ct);
        }
        await db.SaveChangesAsync(ct);
    }
}