using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.ContentModeration;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using EntityComment = UteLearningHub.Domain.Entities.Comment;

namespace UteLearningHub.Application.Features.Comment.Commands.CreateComment;

public class CreateCommentCommandHandler(
    ICommentRepository commentRepository,
    IDocumentRepository documentRepository,
    ICurrentUserService currentUserService,
    ICommentService commentService,
    IDateTimeProvider dateTimeProvider,
    IProfanityFilterService profanityFilterService) : IRequestHandler<CreateCommentCommand, CommentDetailDto>
{
    public async Task<CommentDetailDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create comments");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        // Kiểm tra từ tục tĩu 
        if (profanityFilterService.ContainsProfanity(request.Content))
            throw new BadRequestException("Nội dung bình luận chứa từ ngữ không phù hợp. Vui lòng chỉnh sửa lại.");

        var documentId = await documentRepository.GetIdByDocumentFileIdAsync(request.DocumentFileId, cancellationToken);
        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found");

        if (request.ParentId.HasValue)
        {
            var parentComment = await commentRepository.GetByIdAsync(request.ParentId.Value, disableTracking: true, cancellationToken);
            if (parentComment == null || parentComment.IsDeleted || parentComment.DocumentFileId != request.DocumentFileId)
                throw new NotFoundException($"Parent comment with id {request.ParentId.Value} not found");
        }

        var comment = new EntityComment
        {
            Id = Guid.NewGuid(),
            DocumentFileId = request.DocumentFileId,
            ParentId = request.ParentId,
            Content = request.Content,
            Status = ContentStatus.Approved,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        commentRepository.Add(comment);
        await commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var authorInfo = await commentService.GetCommentAuthorsAsync([userId], cancellationToken);
        var author = authorInfo.TryGetValue(userId, out var authorValue) ? authorValue : null;

        return new CommentDetailDto
        {
            Id = comment.Id,
            DocumentId = documentId.Value,
            DocumentFileId = comment.DocumentFileId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorAvatarUrl = author?.AvatarUrl,
            CreatedById = comment.CreatedById,
            Status = comment.Status,
            ReplyCount = 0,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
