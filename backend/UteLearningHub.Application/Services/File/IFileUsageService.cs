using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Services.File;

public interface IFileUsageService
{
    Task<DomainFile> EnsureFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DomainFile>> EnsureFilesAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default);
    Task<DomainFile?> TryGetByUrlAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(DomainFile file, CancellationToken cancellationToken = default);
}
