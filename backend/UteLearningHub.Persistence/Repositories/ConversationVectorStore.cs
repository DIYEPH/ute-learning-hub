using Microsoft.EntityFrameworkCore;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Persistence.Repositories;

public class ConversationVectorStore : IConversationVectorStore
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public ConversationVectorStore(IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IQueryable<ConversationVector> Query()
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        return dbContext.Set<ConversationVector>().AsQueryable();
    }

    public async Task<ConversationVector?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Set<ConversationVector>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpsertAsync(ConversationVector vector, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Find existing vector by ConversationId (not Id) to properly update
        var existingVector = await dbContext.Set<ConversationVector>()
            .FirstOrDefaultAsync(x => x.ConversationId == vector.ConversationId && x.IsActive, cancellationToken);

        if (existingVector != null)
        {
            // Update existing vector
            existingVector.EmbeddingJson = vector.EmbeddingJson;
            existingVector.CalculatedAt = vector.CalculatedAt;
            dbContext.Set<ConversationVector>().Update(existingVector);
        }
        else
        {
            // Add new vector
            await dbContext.Set<ConversationVector>().AddAsync(vector, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
