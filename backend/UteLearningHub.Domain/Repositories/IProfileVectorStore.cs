using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Domain.Repositories;

public interface IProfileVectorStore
{
    Task UpsertAsync(ProfileVector vector, CancellationToken cancellationToken = default);
    Task<ProfileVector?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<ProfileVector> Query();
}