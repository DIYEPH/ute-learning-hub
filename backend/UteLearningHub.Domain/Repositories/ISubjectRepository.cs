using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface ISubjectRepository : IRepository<Subject, Guid>
{
    Task AddSubjectMajorRelationshipsAsync(Guid subjectId, List<Guid> majorIds, CancellationToken cancellationToken = default);
    Task UpdateSubjectMajorRelationshipsAsync(Guid subjectId, List<Guid> majorIds, CancellationToken cancellationToken = default);
    Task RemoveSubjectMajorRelationshipsAsync(Guid subjectId, CancellationToken cancellationToken = default);
}
