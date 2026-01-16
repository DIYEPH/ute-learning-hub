using UteLearningHub.Application.Common.Sercurity;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using FileStreamResult = UteLearningHub.Application.Common.Results.FileStreamResult;

namespace UteLearningHub.Infrastructure.Services.File;

public class FileAccessService(
    IFileRepository fileRepository,
    IDocumentRepository documentRepository,
    IFileStorageService fileStorageService) : IFileAccessService
{
    public async Task<FileStreamResult> GetFileByIdAsync(Guid fileId, FileRequestType fileType, UserContext user, CancellationToken ct)
    {
        var file = await fileRepository.GetByIdAsync(fileId, cancellationToken: ct);

        if (file == null || file.IsDeleted)
            throw new NotFoundException("File not found");

        if (fileType == FileRequestType.Document)
            await EnsureCanAccessDocumentFile(fileId, user, ct);

        // Ưu tiên pre-signed URL (S3) - redirect, không cần download
        var presignedUrl = fileStorageService.GetPresignedUrl(file.FileUrl);
        if (!string.IsNullOrEmpty(presignedUrl))
            return new FileStreamResult(Stream: null, MimeType: file.MimeType, RedirectUrl: presignedUrl);

        // Fallback: stream trực tiếp (local storage)
        var stream = await fileStorageService.GetFileAsync(file.FileUrl, ct);
        if (stream == null)
            throw new NotFoundException("File content not found");

        return new FileStreamResult(Stream: stream, MimeType: file.MimeType);
    }

    private async Task EnsureCanAccessDocumentFile(Guid fileId, UserContext user, CancellationToken ct)
    {
        var document = await documentRepository.GetByFileIdAsync(fileId, disableTracking: true, ct);
        if (document == null)
            return;

        // Admin bypass, user thường phải approved
        if (!user.IsAdmin)
        {
            var documentFile = await documentRepository.GetDocumentFileByFileIdAsync(fileId, disableTracking: true, ct);
            if (documentFile?.Status != ContentStatus.Approved)
                throw new NotFoundException("File not found");
        }

        // Internal cần đăng nhập
        if (document.Visibility == VisibilityStatus.Internal && !user.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to access this file");
    }
}