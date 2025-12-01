using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Comment.Commands.UpdateComment;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentService _commentService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        ICommentService commentService,
        IDateTimeProvider dateTimeProvider)
    {
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _commentService = commentService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update comments");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var comment = await _commentRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);

        if (comment == null || comment.IsDeleted)
            throw new NotFoundException($"Comment with id {request.Id} not found");

        // Only owner can update
        if (comment.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this comment");

        // Update comment
        comment.Content = request.Content;
        comment.UpdatedById = userId;
        comment.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _commentRepository.UpdateAsync(comment, cancellationToken);
        await _commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get author information
        var authorInfo = await _commentService.GetCommentAuthorsAsync(new[] { userId }, cancellationToken);
        var author = authorInfo.TryGetValue(userId, out var authorValue) ? authorValue : null;

        // Get reply count
        var replyCount = await _commentService.GetReplyCountAsync(comment.Id, cancellationToken);

        return new CommentDto
        {
            Id = comment.Id,
            DocumentId = comment.DocumentFile.DocumentId,
            DocumentFileId = comment.DocumentFileId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorAvatarUrl = author?.AvatarUrl,
            CreatedById = comment.CreatedById,
            ReviewStatus = comment.ReviewStatus,
            ReplyCount = replyCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
