using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Comment;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Report.Queries.GetReports;

public class GetReportsHandler : IRequestHandler<GetReportsQuery, PagedResponse<ReportDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly ICommentService _commentService;

    public GetReportsHandler(
        IReportRepository reportRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        ICommentService commentService)
    {
        _reportRepository = reportRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _commentService = commentService;
    }

    public async Task<PagedResponse<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        // Only admin or moderator can view reports
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view reports");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canView = isAdmin ||
                     (trustLevel.HasValue && trustLevel.Value >= TrustLever.TrustedMember);

        if (!canView)
            throw new UnauthorizedException("Only administrators or TrustedMember+ can view reports");

        var query = _reportRepository.GetQueryableSet()
            .Include(r => r.DocumentFile)
            .AsNoTracking();

        // Filters
        if (request.DocumentFileId.HasValue)
            query = query.Where(r => r.DocumentFileId == request.DocumentFileId.Value);

        if (request.CommentId.HasValue)
            query = query.Where(r => r.CommentId == request.CommentId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(r => r.Content.ToLower().Contains(searchTerm));
        }

        // Order by newest first
        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        // Get reporter information
        var userIds = reports.Select(r => r.CreatedById).Distinct();
        var authorInfo = await _commentService.GetCommentAuthorsAsync(userIds, cancellationToken);

        var reportDtos = reports.Select(r => new ReportDto
        {
            Id = r.Id,
            DocumentFileId = r.DocumentFileId,
            CommentId = r.CommentId,
            TargetUrl = BuildTargetUrl(r.DocumentFile?.DocumentId, r.DocumentFileId, r.CommentId),
            Reason = r.Reason,
            Content = r.Content,
            ReporterName = authorInfo.TryGetValue(r.CreatedById, out var reporter)
                ? reporter.FullName
                : "Unknown",
            ReporterAvatarUrl = authorInfo.TryGetValue(r.CreatedById, out var reporterInfo)
                ? reporterInfo.AvatarUrl
                : null,
            CreatedById = r.CreatedById,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResponse<ReportDto>
        {
            Items = reportDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    private static string? BuildTargetUrl(Guid? documentId, Guid? documentFileId, Guid? commentId)
    {
        if (documentId.HasValue && documentFileId.HasValue)
            return $"/documents/{documentId}/files/{documentFileId}";

        if (commentId.HasValue)
            return null; // Comments don't have direct URL for now

        return null;
    }
}
