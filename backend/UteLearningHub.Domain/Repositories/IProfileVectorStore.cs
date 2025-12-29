using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Domain.Repositories;

public interface IProfileVectorStore
{
    Task UpsertAsync(ProfileVector vector, CancellationToken cancellationToken = default);
    IQueryable<ProfileVector> Query();
}