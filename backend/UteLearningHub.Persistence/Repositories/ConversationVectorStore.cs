using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UteLearningHub.Persistence.Repositories;

public class ConversationVectorStore : IConversationVectorStore
{
    private readonly ApplicationDbContext _dbContext;

    public ConversationVectorStore(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<ConversationVector> Query() => _dbContext.Set<ConversationVector>().AsQueryable();

    public async Task<ConversationVector?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<ConversationVector>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task UpsertAsync(ConversationVector vector, CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.Set<ConversationVector>()
            .AnyAsync(x => x.Id == vector.Id, cancellationToken);

        if (exists)
            _dbContext.Set<ConversationVector>().Update(vector);
        else
            await _dbContext.Set<ConversationVector>().AddAsync(vector, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

