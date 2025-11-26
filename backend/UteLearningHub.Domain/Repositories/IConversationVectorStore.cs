using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Domain.Repositories;

public interface IConversationVectorStore
{
    Task UpsertAsync(ConversationVector vector, CancellationToken cancellationToken = default);
    Task<ConversationVector?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<ConversationVector> Query();
}