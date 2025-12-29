using UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentProgressService(ICurrentUserService currentUserService, IDocumentRepository documentRepository, IUserDocumentProgressRepository userDocumentProgressRepository, IDateTimeProvider dateTimeProvider) : IDocumentProgressService
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly IUserDocumentProgressRepository _userDocumentProgressRepository = userDocumentProgressRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task UpdateAsync(UpdateDocumentProgressCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();


        var documentFileId = request.DocumentFileId;
        var documentId = await _documentRepository.GetIdByDocumentFileIdAsync(documentFileId, ct);

        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {documentFileId} not found");

        var document = await _documentRepository.GetByIdAsync(documentId.Value, true, ct);
        if (document == null)
            throw new NotFoundException("Document not found");

        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(documentFileId, true, ct);
        if (documentFile == null)
            throw new NotFoundException("DocumentFile not found");

        var progress = await _userDocumentProgressRepository.GetByUserAndDocumentFileAsync(userId, documentFileId, cancellationToken: ct);
        var now = _dateTimeProvider.OffsetNow;

        if (progress == null)
        {
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

            _userDocumentProgressRepository.Add(progress);
        }
        else
        {
            progress.LastPage = request.LastPage;
            progress.LastAccessedAt = now;

            if (documentFile != null)
                progress.TotalPages = documentFile.TotalPages;

            _userDocumentProgressRepository.Update(progress);
        }
        await _userDocumentProgressRepository.UnitOfWork.SaveChangesAsync(ct);
    }
}
