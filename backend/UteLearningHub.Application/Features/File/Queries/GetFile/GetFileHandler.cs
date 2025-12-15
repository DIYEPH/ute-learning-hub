using MediatR;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.File.Queries.GetFile;

public class GetFileHandler : IRequestHandler<GetFileQuery, GetFileResponse>
{
    private readonly IFileRepository _fileRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;

    public GetFileHandler(
        IFileRepository fileRepository,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService)
    {
        _fileRepository = fileRepository;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
    }

    public async Task<GetFileResponse> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        // Lấy file từ database
        var file = await _fileRepository.GetByIdAsync(request.FileId, disableTracking: true, cancellationToken);
        if (file == null || file.IsDeleted)
            throw new NotFoundException("File not found");

        // Tìm document chứa file này (nếu có)
        var document = await _documentRepository.GetByFileIdAsync(request.FileId, disableTracking: true, cancellationToken);

        // Nếu file thuộc document, cần kiểm tra quyền truy cập
        if (document != null)
        {
            var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
            var isAuthenticated = _currentUserService.IsAuthenticated;
            var userId = _currentUserService.UserId;

            // Lấy DocumentFile để kiểm tra trạng thái review
            var documentFile = await _documentRepository.GetDocumentFileByIdAsync(request.FileId, disableTracking: true, cancellationToken);
            
            // Non-admin users can only access approved files
            if (!isAdmin && documentFile != null && documentFile.Status != ContentStatus.Approved)
                throw new NotFoundException("File not found");

            // Kiểm tra visibility
            if (document.Visibility == VisibilityStatus.Internal)
            {
                // Internal: cần authenticated
                if (!isAuthenticated)
                {
                    throw new UnauthorizedException("You must be authenticated to access this file");
                }
            }
            // Public: ai cũng xem được
        }

        // Lấy file stream từ storage
        var stream = await _fileStorageService.GetFileAsync(file.FileUrl, cancellationToken);
        if (stream == null)
            throw new NotFoundException("File content not found");

        return new GetFileResponse
        {
            FileStream = stream,
            MimeType = file.MimeType
        };
    }
}

