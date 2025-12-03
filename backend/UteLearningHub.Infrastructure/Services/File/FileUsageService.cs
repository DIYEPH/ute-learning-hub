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
            // Đọc với disableTracking để không giữ entity trong DbContext -> tránh conflict khi update sau đó
            var file = await _fileRepository.GetByIdAsync(fileId, disableTracking: true, cancellationToken);
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
                .AsNoTracking()
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

        public async Task MarkFilesAsPermanentAsync(IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default)
        {
            var ids = fileIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return;

            // Lấy các file cần cập nhật với tracking từ DbContext, không dùng entity đã đọc trước đó
            var existingFiles = await _fileRepository.GetQueryableSet()
                .Where(f => ids.Contains(f.Id) && !f.IsDeleted && f.IsTemporary)
                .ToListAsync(cancellationToken);

            if (!existingFiles.Any())
                return;

            foreach (var file in existingFiles)
            {
                file.IsTemporary = false;
            }

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


