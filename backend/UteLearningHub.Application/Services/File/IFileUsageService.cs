using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Application.Services.File;

public interface IFileUsageService
{
    Task<Domain.Entities.File> EnsureFileAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Entities.File>> EnsureFilesAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default);
    Task<Domain.Entities.File?> TryGetByUrlAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task MarkFilesAsPermanentAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(Domain.Entities.File file, CancellationToken cancellationToken = default);
}


