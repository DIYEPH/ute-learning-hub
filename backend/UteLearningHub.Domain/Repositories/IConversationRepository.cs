using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IConversationRepository : IRepository<Conversation, Guid>
{
    IQueryable<ConversationJoinRequest> GetJoinRequestsQueryable();
    IQueryable<ConversationInvitation> GetInvitationsQueryable();
    Task<Conversation?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default);
    Task AddMemberAsync(ConversationMember member, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task<ConversationMember?> GetDeletedMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default);
    Task RestoreMemberAsync(ConversationMember member, CancellationToken cancellationToken = default);
    Task AddJoinRequestAsync(ConversationJoinRequest joinRequest, CancellationToken cancellationToken = default);
    Task AddInvitationAsync(ConversationInvitation invitation, CancellationToken cancellationToken = default);
    Task<ConversationInvitation?> GetInvitationByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

