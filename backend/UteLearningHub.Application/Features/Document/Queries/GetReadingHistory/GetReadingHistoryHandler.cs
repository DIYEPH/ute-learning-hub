using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetReadingHistory;

public class GetReadingHistoryHandler : IRequestHandler<GetReadingHistoryQuery, PagedResponse<ReadingHistoryItemDto>>
{
    private readonly IUserDocumentProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetReadingHistoryHandler(
        IUserDocumentProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<ReadingHistoryItemDto>> Handle(GetReadingHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to get reading history");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var query = _progressRepository.GetQueryableSet()
            .Where(p => p.UserId == userId)
            .Include(p => p.Document)
                .ThenInclude(d => d.Subject)
            .Include(p => p.Document)
                .ThenInclude(d => d.DocumentFiles)
            .Include(p => p.Document)
                .ThenInclude(d => d.Reviews)
            .Include(p => p.DocumentFile)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var progressList = await query
            .OrderByDescending(p => p.LastAccessedAt)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var items = progressList
            .Where(p => p.Document != null && !p.Document.IsDeleted && p.DocumentFile != null)
            .Select(p => new ReadingHistoryItemDto
            {
                DocumentId = p.DocumentId,
                DocumentName = p.Document!.DocumentName,
                DocumentFileId = p.DocumentFileId,
                FileTitle = p.DocumentFile?.Title,
                LastPage = p.LastPage,
                TotalPages = p.TotalPages,
                LastAccessedAt = p.LastAccessedAt,
                CoverFileId = p.Document.CoverFileId,
                SubjectName = p.Document.Subject?.SubjectName,
                TotalViewCount = p.Document.DocumentFiles.Sum(f => f.ViewCount),
                UsefulCount = p.Document.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
                NotUsefulCount = p.Document.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful)
            })
            .ToList();

        return new PagedResponse<ReadingHistoryItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
