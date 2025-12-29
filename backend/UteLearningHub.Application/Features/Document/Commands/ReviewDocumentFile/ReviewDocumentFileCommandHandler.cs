using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.ReviewDocumentFile;

public class ReviewDocumentFileCommandHandler : IRequestHandler<ReviewDocumentFileCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReviewDocumentFileCommandHandler(
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

    public async Task Handle(ReviewDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review document files");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var isAdmin = _currentUserService.IsInRole("Admin");

        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canReview = isAdmin ||
                       trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator;

        if (!canReview)
            throw new UnauthorizedException("Only administrators or moderators can review document files");

        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(
            request.DocumentFileId,
            disableTracking: false,
            cancellationToken);

        if (documentFile == null || documentFile.IsDeleted)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found");

        // Update status and review info
        documentFile.Status = request.Status;
        documentFile.ReviewedById = userId;
        documentFile.ReviewedAt = _dateTimeProvider.OffsetNow;
        documentFile.ReviewNote = request.Reason;

        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
