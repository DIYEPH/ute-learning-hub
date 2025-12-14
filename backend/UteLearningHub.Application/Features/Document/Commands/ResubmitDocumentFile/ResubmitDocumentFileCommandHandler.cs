using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.ResubmitDocumentFile;

public class ResubmitDocumentFileCommandHandler : IRequestHandler<ResubmitDocumentFileCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResubmitDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(ResubmitDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to resubmit document files");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: false, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        // Only the owner can resubmit their document
        if (document.CreatedById != userId)
            throw new UnauthorizedException("You can only resubmit your own documents");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == request.DocumentFileId && !df.IsDeleted);

        if (fileEntity == null)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found in this document");

        // Only hidden files can be resubmitted
        if (fileEntity.Status != ContentStatus.Hidden)
            throw new BadRequestException("Only hidden document files can be resubmitted for review");

        // Change status to PendingReview
        fileEntity.Status = ContentStatus.PendingReview;
        fileEntity.UpdatedById = userId;
        fileEntity.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
