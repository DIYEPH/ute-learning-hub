using Microsoft.EntityFrameworkCore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;

namespace UteLearningHub.Persistence.Repositories;

public class ConversationRepository : Repository<Conversation, Guid>, IConversationRepository
{
    public ConversationRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider)
        : base(dbContext, dateTimeProvider)
    {
    }

    public IQueryable<ConversationJoinRequest> GetJoinRequestsQueryable()
    {
        return _dbContext.Set<ConversationJoinRequest>();
    }

    public async Task<Conversation?> GetByIdWithDetailsAsync(Guid id, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        var query = GetQueryableSet()
            .Include(c => c.Subject)
            .Include(c => c.Members)
            .Include(c => c.ConversationJoinRequests)
            .Include(c => c.ConversationTags)
                .ThenInclude(ct => ct.Tag)
            .Where(c => c.Id == id);

        if (disableTracking)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddMemberAsync(ConversationMember member, CancellationToken cancellationToken = default)
    {
        member.CreatedAt = _dateTimeProvider.OffsetNow;
        await _dbContext.ConversationMembers.AddAsync(member, cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId && !m.IsDeleted, cancellationToken);

        if (member != null)
        {
            member.IsDeleted = true;
            member.DeletedAt = _dateTimeProvider.OffsetNow;
            member.DeletedById = userId; // User tự xóa chính mình
            _dbContext.ConversationMembers.Update(member);
        }
    }

    public async Task<ConversationMember?> GetDeletedMemberAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConversationMembers
            .FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.UserId == userId && m.IsDeleted, cancellationToken);
    }

    public async Task RestoreMemberAsync(ConversationMember member, CancellationToken cancellationToken = default)
    {
        member.IsDeleted = false;
        member.DeletedAt = null;
        member.DeletedById = null;
        member.UpdatedAt = _dateTimeProvider.OffsetNow;
        _dbContext.ConversationMembers.Update(member);
        await Task.CompletedTask;
    }

    public async Task AddJoinRequestAsync(ConversationJoinRequest joinRequest, CancellationToken cancellationToken = default)
    {
        await _dbContext.Set<ConversationJoinRequest>().AddAsync(joinRequest, cancellationToken);
    }

    public IQueryable<ConversationInvitation> GetInvitationsQueryable()
    {
        return _dbContext.ConversationInvitations;
    }

    public async Task AddInvitationAsync(ConversationInvitation invitation, CancellationToken cancellationToken = default)
    {
        await _dbContext.ConversationInvitations.AddAsync(invitation, cancellationToken);
    }

    public async Task<ConversationInvitation?> GetInvitationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ConversationInvitations
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);
    }
}

