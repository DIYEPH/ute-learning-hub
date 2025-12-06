using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Repositories.Base;

namespace UteLearningHub.Domain.Repositories;

public interface IUserDocumentProgressRepository : IRepository<UserDocumentProgress, Guid>
{
    Task<UserDocumentProgress?> GetByUserAndDocumentFileAsync(
        Guid userId, 
        Guid documentFileId, 
        bool disableTracking = false, 
        CancellationToken cancellationToken = default);
    
    Task<IList<UserDocumentProgress>> GetByUserAndDocumentAsync(
        Guid userId, 
        Guid documentId, 
        bool disableTracking = false, 
        CancellationToken cancellationToken = default);
}

