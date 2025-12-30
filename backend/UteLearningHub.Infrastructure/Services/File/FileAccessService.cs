using UteLearningHub.Application.Common.Sercurity;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using FileStreamResult = UteLearningHub.Application.Common.Results.FileStreamResult;

namespace UteLearningHub.Infrastructure.Services.File;

public class FileAccessService : IFileAccessService
{
    private readonly IFileRepository _fileRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public FileAccessService(
        IFileRepository fileRepository,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorage)
    {
        _fileRepository = fileRepository;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorage;
    }

    public async Task<FileStreamResult> GetFileByIdAsync(Guid fileId, FileRequestType fileType, UserContext user, CancellationToken ct)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken: ct);

        if (file == null || file.IsDeleted)
            throw new NotFoundException("File not found");

        if (fileType == FileRequestType.Document)
        {
            await EnsureCanAccessDocumentFile(fileId, user, ct);
        }

        // Try to get a pre-signed URL first (for S3 storage)
        var presignedUrl = _fileStorageService.GetPresignedUrl(file.FileUrl);
        if (!string.IsNullOrEmpty(presignedUrl))
        {
            // Return redirect response - no need to download the file
            return new FileStreamResult(
                Stream: null,
                MimeType: file.MimeType,
                RedirectUrl: presignedUrl
            );
        }

        // Fall back to streaming (for local storage or if pre-signed URL fails)
        var stream = await _fileStorageService.GetFileAsync(file.FileUrl, ct);
        if (stream == null)
            throw new NotFoundException("File content not found");

        return new FileStreamResult(
            Stream: stream,
            MimeType: file.MimeType
        );
    }

    private async Task EnsureCanAccessDocumentFile(Guid fileId, UserContext user, CancellationToken ct)
    {
        var document = await _documentRepository.GetByFileIdAsync(fileId, disableTracking: true, ct);
        if (document == null)
            return;

        if (!user.IsAdmin)
        {
            var documentFile = await _documentRepository
                .GetDocumentFileByFileIdAsync(fileId, disableTracking: true, ct);

            if (documentFile?.Status != ContentStatus.Approved)
                throw new NotFoundException("File not found");
        }

        if (document.Visibility == VisibilityStatus.Internal && !user.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "You must be authenticated to access this file");
        }
    }
}

