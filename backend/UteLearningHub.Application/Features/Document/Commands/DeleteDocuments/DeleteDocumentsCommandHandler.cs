using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public class DeleteDocumentsCommandHandler : IRequestHandler<DeleteDocumentsCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteDocumentsCommandHandler(
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Unit> Handle(DeleteDocumentsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        if (document.IsDeleted)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Check permission: only owner or admin can delete
        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");

        if (!isOwner && !isAdmin)
            throw new UnauthorizedException("You don't have permission to delete this document");

        // Soft delete
        document.IsDeleted = true;
        document.DeletedAt = _dateTimeProvider.OffsetNow;
        document.DeletedById = userId;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}