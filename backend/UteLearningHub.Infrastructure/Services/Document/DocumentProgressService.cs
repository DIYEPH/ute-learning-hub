using UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentProgressService(
    ICurrentUserService currentUserService,
    IDocumentRepository documentRepository,
    IUserDocumentProgressRepository progressRepository,
    IDateTimeProvider dateTimeProvider) : IDocumentProgressService
{
    public async Task UpdateAsync(UpdateDocumentProgressCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var documentFileId = request.DocumentFileId;

        var documentId = await documentRepository.GetIdByDocumentFileIdAsync(documentFileId, ct)
            ?? throw new NotFoundException($"Document file with id {documentFileId} not found");

        var documentFile = await documentRepository.GetDocumentFileByIdAsync(documentFileId, true, ct)
            ?? throw new NotFoundException("DocumentFile not found");

        var progress = await progressRepository.GetByUserAndDocumentFileAsync(userId, documentFileId, cancellationToken: ct);
        var now = dateTimeProvider.OffsetNow;

        if (progress == null)
        {
            progress = new UserDocumentProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                DocumentId = documentId,
                DocumentFileId = documentFileId,
                LastPage = request.LastPage,
                TotalPages = documentFile.TotalPages,
                LastAccessedAt = now
            };
            progressRepository.Add(progress);
        }
        else
        {
            progress.LastPage = request.LastPage;
            progress.LastAccessedAt = now;
            progress.TotalPages = documentFile.TotalPages;
            progressRepository.Update(progress);
        }

        await progressRepository.UnitOfWork.SaveChangesAsync(ct);
    }
}