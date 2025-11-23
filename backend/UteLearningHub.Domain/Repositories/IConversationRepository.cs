using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;
using System.Linq;

namespace UteLearningHub.Domain.Repositories;

public interface IConversationRepository : IRepository<Conversation, Guid>
{
    IQueryable<ConversationJoinRequest> GetJoinRequestsQueryable();
    Task<Conversation?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
}
