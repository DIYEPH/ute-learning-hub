using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.ReviewDocument;

public class ReviewDocumentCommandHandler : IRequestHandler<ReviewDocumentCommand, Unit>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewDocumentCommandHandler(
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

    public async Task<Unit> Handle(ReviewDocumentCommand request, CancellationToken cancellationToken)
    {
        // Only admin can review documents
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var isAdmin = _currentUserService.IsInRole("Admin");

        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canReview = isAdmin || 
                       (trustLevel.HasValue && 
                        (trustLevel.Value >= TrustLever.Moderator));

        if (!canReview)
            throw new UnauthorizedException("Only administrators or users with high trust level can review documents");

        var document = await _documentRepository.GetByIdAsync(request.DocumentId, disableTracking: false, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        if (document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        // Update review information
        document.ReviewStatus = request.ReviewStatus;
        document.ReviewedById = userId;
        document.ReviewedAt = _dateTimeProvider.OffsetNow;
        document.ReviewNote = request.ReviewNote;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
