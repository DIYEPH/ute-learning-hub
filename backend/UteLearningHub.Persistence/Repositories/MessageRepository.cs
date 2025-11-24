using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class MessageRepository : Repository<Message, Guid>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider) { }

    public async Task<Message?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
        .Include(m => m.MessageFiles)
        .ThenInclude(mf => mf.File);

        var finalQuery = disableTracking ? query.AsNoTracking() : query;

        return await finalQuery.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public IQueryable<Message> GetQueryableWithDetails()
    {
        return GetQueryableSet()
            .Include(m => m.MessageFiles)
                .ThenInclude(mf => mf.File);
    }
}