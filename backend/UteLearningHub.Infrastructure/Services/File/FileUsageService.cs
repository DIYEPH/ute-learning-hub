using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Infrastructure.Services.File;

public class FileUsageService(IFileRepository fileRepository) : IFileUsageService
{
    public async Task<DomainFile> EnsureFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await fileRepository.GetByIdAsync(fileId, disableTracking: false, cancellationToken);
        if (file == null)
            throw new NotFoundException($"File with id {fileId} not found");

        return file;
    }

    public async Task<IReadOnlyList<DomainFile>> EnsureFilesAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var distinctIds = fileIds.Distinct().ToList();
        if (!distinctIds.Any())
            return Array.Empty<DomainFile>();

        var files = await fileRepository.GetQueryableSet()
            .AsNoTracking()
            .Where(f => distinctIds.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        // Kiểm tra tất cả file tồn tại, nếu thiếu thì throw
        if (files.Count != distinctIds.Count)
            throw new("Một hoặc nhiều tệp không tồn tại");

        return files;
    }

    public async Task<DomainFile?> TryGetByUrlAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return null;

        return await fileRepository.GetQueryableSet()
            .Where(f => f.FileUrl == fileUrl && !f.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task DeleteFileAsync(DomainFile file, CancellationToken cancellationToken = default)
    {
        if (file == null)
            return;

        fileRepository.Delete(file);
        await fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}