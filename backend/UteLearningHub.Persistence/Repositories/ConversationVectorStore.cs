using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Persistence.Repositories;

public class ConversationVectorStore(IDbContextFactory<ApplicationDbContext> dbFactory) : IConversationVectorStore
{
    public IQueryable<ConversationVector> Query()
        => dbFactory.CreateDbContext().Set<ConversationVector>().AsQueryable();

    public async Task<ConversationVector?> GetAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.Set<ConversationVector>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task UpsertAsync(ConversationVector vector, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var existing = await db.Set<ConversationVector>()
            .FirstOrDefaultAsync(x => x.ConversationId == vector.ConversationId && x.IsActive, ct);

        if (existing != null)
        {
            existing.EmbeddingJson = vector.EmbeddingJson;
            existing.CalculatedAt = vector.CalculatedAt;
        }
        else
            await db.Set<ConversationVector>().AddAsync(vector, ct);
        await db.SaveChangesAsync(ct);
    }
}