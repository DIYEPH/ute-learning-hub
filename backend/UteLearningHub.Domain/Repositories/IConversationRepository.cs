using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IConversationRepository : IRepository<Conversation, Guid>
{
    IQueryable<ConversationJoinRequest> GetJoinRequestsQueryable();
    Task<Conversation?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task AddMemberAsync(ConversationMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationMember?> GetDeletedMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task RestoreMemberAsync(ConversationMember member, CancellationToken cancellationToken = default);
}
