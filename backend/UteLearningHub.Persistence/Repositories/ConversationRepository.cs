using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
}
