using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Comment.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete comments");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var comment = await _commentRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (comment == null || comment.IsDeleted)
            throw new NotFoundException($"Comment with id {request.Id} not found");

        // Check permission: owner, admin, or user with high trust level
        var isOwner = comment.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canDelete = isOwner ||
                       isAdmin ||
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this comment");

        // Soft delete
        comment.IsDeleted = true;
        comment.DeletedById = userId;
        comment.DeletedAt = _dateTimeProvider.OffsetUtcNow;
        _commentRepository.Update(comment);
        await _commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
