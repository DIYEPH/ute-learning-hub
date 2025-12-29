using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Comment.Queries.GetDocumentComments;

public class GetDocumentCommentsHandler : IRequestHandler<GetDocumentCommentsQuery, PagedResponse<CommentDetailDto>>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICommentService _commentService;

    public GetDocumentCommentsHandler(
        ICommentRepository commentRepository,
        ICurrentUserService currentUserService,
        ICommentService commentService)
    {
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
        _commentService = commentService;
    }

    public async Task<PagedResponse<CommentDetailDto>> Handle(GetDocumentCommentsQuery request, CancellationToken cancellationToken)
    {
        var query = _commentRepository.GetQueryableSet()
            .Include(c => c.DocumentFile)
                .ThenInclude(df => df.Document)
            .AsNoTracking()
            .Where(c => c.DocumentFile.DocumentId == request.DocumentId);

        if (request.ParentId.HasValue)
            query = query.Where(c => c.ParentId == request.ParentId.Value);
        else
            query = query.Where(c => c.ParentId == null);

        // Only show approved comments for public users, admin có thể xem tất cả
        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            query = query.Where(c => c.Status == ContentStatus.Approved);

        if (request.ParentId.HasValue)
            query = query.OrderByDescending(c => c.CreatedAt);
        else
            query = query.OrderBy(c => c.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var comments = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(c => new CommentDetailDto
            {
                Id = c.Id,
                DocumentId = c.DocumentFile.DocumentId,
                DocumentFileId = c.DocumentFileId,
                ParentId = c.ParentId,
                Content = c.Content,
                CreatedById = c.CreatedById,
                Status = c.Status,
                ReplyCount = c.Childrens.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // Get author information for all comments
        var userIds = comments.Select(c => c.CreatedById).Distinct();
        var authorInfo = await _commentService.GetCommentAuthorsAsync(userIds, cancellationToken);

        // Map author information to comments
        var commentDtos = comments.Select(c => new CommentDetailDto
        {
            Id = c.Id,
            DocumentId = c.DocumentId,
            DocumentFileId = c.DocumentFileId,
            ParentId = c.ParentId,
            Content = c.Content,
            AuthorName = authorInfo.TryGetValue(c.CreatedById, out var author)
                ? author.FullName
                : "Unknown",
            AuthorAvatarUrl = authorInfo.TryGetValue(c.CreatedById, out var authorInfoValue)
                ? authorInfoValue.AvatarUrl
                : null,
            CreatedById = c.CreatedById,
            Status = c.Status,
            ReplyCount = c.ReplyCount,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return new PagedResponse<CommentDetailDto>
        {
            Items = commentDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}


