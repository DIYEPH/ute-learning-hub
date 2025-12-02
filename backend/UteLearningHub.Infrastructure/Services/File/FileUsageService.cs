using Microsoft.EntityFrameworkCore;
using System.Linq;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.File;

public class FileUsageService : IFileUsageService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;

    public FileUsageService(
        IFileRepository fileRepository,
        IFileStorageService fileStorageService)
    {
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Domain.Entities.File> EnsureFileAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, disableTracking: false, cancellationToken);
        if (file == null || file.IsDeleted)
            throw new NotFoundException($"File with id {fileId} not found");

        return file;
    }

    public async Task<IReadOnlyList<Domain.Entities.File>> EnsureFilesAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default)
    {
        var distinctIds = fileIds.Distinct().ToList();
        if (!distinctIds.Any())
            return Array.Empty<Domain.Entities.File>();

        var files = await _fileRepository.GetQueryableSet()
            .Where(f => distinctIds.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        if (files.Count != distinctIds.Count)
            throw new NotFoundException("Một hoặc nhiều tệp không tồn tại");

        return files;
    }

    public async Task<Domain.Entities.File?> TryGetByUrlAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return null;

        return await _fileRepository.GetQueryableSet()
            .Where(f => f.FileUrl == fileUrl && !f.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task MarkFilesAsPermanentAsync(IEnumerable<Domain.Entities.File> files, CancellationToken cancellationToken = default)
    {
        // Lấy danh sách Id duy nhất, bỏ null
        var ids = files
            .Where(f => f != null)
            .Select(f => f.Id)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return;

        // Reload từ database để tránh vấn đề RowVersion / concurrency khi dùng BulkUpdate
        var existingFiles = await _fileRepository.GetQueryableSet()
            .Where(f => ids.Contains(f.Id) && !f.IsDeleted)
            .ToListAsync(cancellationToken);

        if (!existingFiles.Any())
            return;

        foreach (var file in existingFiles.Where(f => f.IsTemporary))
        {
            file.IsTemporary = false;
        }

        // Số lượng file ít, nên dùng SaveChanges thông thường để tránh DbUpdateConcurrencyException khi BulkUpdate
        await _fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteFileAsync(Domain.Entities.File file, CancellationToken cancellationToken = default)
    {
        if (file == null)
            return;

        await _fileRepository.DeleteAsync(file, cancellationToken);
        await _fileRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        await _fileStorageService.DeleteFileAsync(file.FileUrl, cancellationToken);
    }
}


