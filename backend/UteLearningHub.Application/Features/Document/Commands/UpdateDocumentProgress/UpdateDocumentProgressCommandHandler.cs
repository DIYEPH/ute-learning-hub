using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public class UpdateDocumentProgressCommandHandler : IRequestHandler<UpdateDocumentProgressCommand, Unit>
{
    private readonly IUserDocumentProgressRepository _progressRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateDocumentProgressCommandHandler(
        IUserDocumentProgressRepository progressRepository,
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _progressRepository = progressRepository;
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(UpdateDocumentProgressCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update document progress");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate document file exists
        var documentFileId = request.DocumentFileId;
        var documentId = await _documentRepository.GetDocumentIdByDocumentFileIdAsync(documentFileId, cancellationToken);

        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {documentFileId} not found");

        // Validate document is accessible
        var document = await _documentRepository.GetByIdAsync(documentId.Value, disableTracking: true, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new NotFoundException("Document not found");

        var isAdmin = _currentUserService.IsInRole("Admin");

        // Lấy DocumentFile để kiểm tra review status và TotalPages
        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(documentFileId, disableTracking: true, cancellationToken);
        
        // Check file-level status
        if (!isAdmin && documentFile != null && documentFile.Status != Domain.Constaints.Enums.ContentStatus.Approved)
            throw new NotFoundException("Document file not found");

        // Tìm hoặc tạo progress
        var progress = await _progressRepository.GetByUserAndDocumentFileAsync(
            userId,
            documentFileId,
            disableTracking: false,
            cancellationToken);

        var now = _dateTimeProvider.OffsetNow;

        if (progress == null)
        {
            // Tạo mới progress
            progress = new UserDocumentProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DocumentId = documentId.Value,
                DocumentFileId = documentFileId,
                LastPage = request.LastPage,
                TotalPages = documentFile?.TotalPages,
                LastAccessedAt = now
            };
            await _progressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            // Update existing progress
            progress.LastPage = request.LastPage;
            progress.LastAccessedAt = now;
            // Cập nhật TotalPages nếu DocumentFile có thay đổi
            if (documentFile != null)
            {
                progress.TotalPages = documentFile.TotalPages;
            }
            await _progressRepository.UpdateAsync(progress, cancellationToken);
        }

        await _progressRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

