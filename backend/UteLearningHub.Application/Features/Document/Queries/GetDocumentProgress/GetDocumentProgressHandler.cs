using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentProgress;

public class GetDocumentProgressHandler : IRequestHandler<GetDocumentProgressQuery, DocumentFileProgressDto>
{
    private readonly IUserDocumentProgressRepository _progressRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDocumentProgressHandler(
        IUserDocumentProgressRepository progressRepository,
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService)
    {
        _progressRepository = progressRepository;
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DocumentFileProgressDto> Handle(GetDocumentProgressQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to get document progress");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate document file exists
        var documentFileId = request.DocumentFileId;
        var documentId = await _documentRepository.GetIdByDocumentFileIdAsync(documentFileId, cancellationToken);

        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {documentFileId} not found");

        // Validate document is accessible
        var document = await _documentRepository.GetByIdAsync(documentId.Value, disableTracking: true, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new NotFoundException("Document not found");

        var isAdmin = _currentUserService.IsInRole("Admin");

        // Check file-level status
        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(documentFileId, disableTracking: true, cancellationToken);
        if (!isAdmin && documentFile != null && documentFile.Status != Domain.Constaints.Enums.ContentStatus.Approved)
            throw new NotFoundException("Document file not found");


        // Lấy progress
        var progress = await _progressRepository.GetByUserAndDocumentFileAsync(
            userId,
            documentFileId,
            disableTracking: true,
            cancellationToken);

        if (progress == null)
        {
            return new DocumentFileProgressDto
            {
                DocumentFileId = documentFileId,
                LastPage = 1,
                TotalPages = null,
                LastAccessedAt = null
            };
        }

        return new DocumentFileProgressDto
        {
            DocumentFileId = documentFileId,
            LastPage = progress.LastPage,
            TotalPages = progress.TotalPages,
            LastAccessedAt = progress.LastAccessedAt
        };
    }
}

