using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class EventRepository : Repository<Event, Guid>, IEventRepository
{
    public EventRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
    {
    }

    public IQueryable<Event> GetActiveEvents(int? take = null)
    {
        var now = _dateTimeProvider.OffsetNow;

        IQueryable<Event> query = GetQueryableSet()
            .Where(e => !e.IsDeleted
                        && e.IsActive
                        && e.StartAt <= now
                        && e.EndAt >= now)
            .OrderByDescending(e => e.Priority)
            .ThenBy(e => e.StartAt);

        if (take.HasValue)
            query = query.Take(take.Value);

        return query;
    }
}