using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.ContentModeration;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Comment.Commands.UpdateComment;

public class UpdateCommentCommandHandler(
    ICommentRepository commentRepository,
    ICurrentUserService currentUserService,
    ICommentService commentService,
    IDateTimeProvider dateTimeProvider,
    IProfanityFilterService profanityFilterService) : IRequestHandler<UpdateCommentCommand, CommentDetailDto>
{
    public async Task<CommentDetailDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update comments");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var comment = await commentRepository.GetByIdAsync(request.Id, disableTracking: false, cancellationToken);
        if (comment == null || comment.IsDeleted)
            throw new NotFoundException($"Comment with id {request.Id} not found");

        // Chỉ owner mới được update
        if (comment.CreatedById != userId)
            throw new UnauthorizedException("You don't have permission to update this comment");

        // Kiểm tra từ tục tĩu khi update (chống bypass)
        if (profanityFilterService.ContainsProfanity(request.Content))
            throw new BadRequestException("Nội dung bình luận chứa từ ngữ không phù hợp. Vui lòng chỉnh sửa lại.");

        comment.Content = request.Content;
        comment.UpdatedById = userId;
        comment.UpdatedAt = dateTimeProvider.OffsetNow;

        commentRepository.Update(comment);
        await commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var authorInfo = await commentService.GetCommentAuthorsAsync([userId], cancellationToken);
        var author = authorInfo.TryGetValue(userId, out var authorValue) ? authorValue : null;
        var replyCount = await commentService.GetReplyCountAsync(comment.Id, cancellationToken);

        return new CommentDetailDto
        {
            Id = comment.Id,
            DocumentId = comment.DocumentFile.DocumentId,
            DocumentFileId = comment.DocumentFileId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorAvatarUrl = author?.AvatarUrl,
            CreatedById = comment.CreatedById,
            Status = comment.Status,
            ReplyCount = replyCount,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
