using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public class DeleteDocumentsCommandHandler : IRequestHandler<DeleteDocumentsCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteDocumentsCommandHandler(
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _userService = userService;
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

        // Check permission: only owner, admin or users with high trust level can delete
        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canDelete = isOwner ||
                       isAdmin ||
                       (trustLevel.HasValue &&
                        (trustLevel.Value >= TrustLever.Moderator));

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this document");

        // Soft delete
        await _documentRepository.DeleteAsync(document, userId, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}