using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using EntityComment = UteLearningHub.Domain.Entities.Comment;

namespace UteLearningHub.Application.Features.Comment.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentService _commentService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCommentCommandHandler(
        ICommentRepository commentRepository,
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        ICommentService commentService,
        IDateTimeProvider dateTimeProvider)
    {
        _commentRepository = commentRepository;
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _commentService = commentService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create comments");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Validate document file exists and belongs to a non-deleted document
        var documentId = await _documentRepository.GetDocumentIdByDocumentFileIdAsync(request.DocumentFileId, cancellationToken);

        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found");

        // If ParentId is provided, validate parent comment exists
        if (request.ParentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentId.Value, disableTracking: true, cancellationToken);
            if (parentComment == null || parentComment.IsDeleted || parentComment.DocumentFileId != request.DocumentFileId)
                throw new NotFoundException($"Parent comment with id {request.ParentId.Value} not found");
        }

        // Create comment
        var comment = new EntityComment
        {
            Id = Guid.NewGuid(),
            DocumentFileId = request.DocumentFileId,
            ParentId = request.ParentId,
            Content = request.Content,
            ReviewStatus = ReviewStatus.Approved, // Comments are auto-approved, or can be PendingReview
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Get author information
        var authorInfo = await _commentService.GetCommentAuthorsAsync(new[] { userId }, cancellationToken);
        var author = authorInfo.TryGetValue(userId, out var authorValue) ? authorValue : null;

        return new CommentDto
        {
            Id = comment.Id,
            DocumentId = documentId.Value,
            DocumentFileId = comment.DocumentFileId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorAvatarUrl = author?.AvatarUrl,
            CreatedById = comment.CreatedById,
            ReviewStatus = comment.ReviewStatus,
            ReplyCount = 0,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
