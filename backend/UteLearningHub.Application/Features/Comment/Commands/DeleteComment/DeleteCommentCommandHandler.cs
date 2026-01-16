using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Comment.Commands.DeleteComment;

public class DeleteCommentCommandHandler(
    ICommentRepository commentRepository,
    ICurrentUserService currentUserService,
    IUserService userService,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<DeleteCommentCommand>
{
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete comments");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var comment = await commentRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);
        if (comment == null || comment.IsDeleted)
            throw new NotFoundException($"Comment with id {request.Id} not found");

        // Kiểm tra quyền: Owner, Admin hoặc Moderator trở lên
        var isOwner = comment.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, cancellationToken);

        if (!isOwner && !isAdmin && (!trustLevel.HasValue || trustLevel.Value < TrustLever.Moderator))
            throw new UnauthorizedException("You don't have permission to delete this comment");

        // Soft delete
        comment.IsDeleted = true;
        comment.DeletedById = userId;
        comment.DeletedAt = dateTimeProvider.OffsetUtcNow;
        commentRepository.Update(comment);
        await commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
    }
}
