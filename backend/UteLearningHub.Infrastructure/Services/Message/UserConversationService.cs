using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Message;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.Message;

public class UserConversationService : IUserConversationService
{
    private readonly IConversationRepository _conversationRepository;

    public UserConversationService(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public async Task<List<Guid>> GetUserConversationIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationRepository.GetQueryableSet()
            .Include(c => c.Members)
            .AsNoTracking()
            .Where(c => !c.IsDeleted 
                && c.ConversationStatus == Domain.Constaints.Enums.ConversationStatus.Active
                && c.Members.Any(m => m.UserId == userId && !m.IsDeleted))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return conversations;
    }
}