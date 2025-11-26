using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IEventRepository : IRepository<Event, Guid>
{
    IQueryable<Event> GetActiveEvents(int? take = null);
}
