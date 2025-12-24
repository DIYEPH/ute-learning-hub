using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetMyDocuments;

public class GetMyDocumentsHandler : IRequestHandler<GetMyDocumentsQuery, PagedResponse<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyDocumentsHandler(
        IDocumentRepository documentRepository,
        IDocumentReviewRepository documentReviewRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _documentReviewRepository = documentReviewRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<DocumentDto>> Handle(GetMyDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view your documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException("User ID not found");

        var query = _documentRepository.GetQueryableWithIncludes()
            .AsNoTracking()
            .Where(d => d.CreatedById == userId);

        if (request.SubjectId.HasValue)
            query = query.Where(d => d.SubjectId == request.SubjectId.Value);

        if (request.TypeId.HasValue)
            query = query.Where(d => d.TypeId == request.TypeId.Value);

        if (request.TagId.HasValue)
            query = query.Where(d => d.DocumentTags.Any(dt => dt.TagId == request.TagId.Value));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(searchTerm) ||
                d.Description.ToLower().Contains(searchTerm));
        }

        if (request.Visibility.HasValue)
            query = query.Where(d => d.Visibility == request.Visibility.Value);

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(d => d.DocumentName)
                : query.OrderBy(d => d.DocumentName),
            // "author" sort tạm bỏ, sẽ dùng bảng Author/DocumentAuthor sau
            "createdat" or "date" => request.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var documentIds = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(d => d.Id)
            .ToListAsync(cancellationToken);

        // Get review stats for all documents
        var reviewStats = await _documentReviewRepository.GetQueryableSet()
            .Where(dr => documentIds.Contains(dr.DocumentId))
            .GroupBy(dr => new { dr.DocumentId, dr.DocumentReviewType })
            .Select(g => new
            {
                DocumentId = g.Key.DocumentId,
                DocumentReviewType = g.Key.DocumentReviewType,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var reviewStatsDict = reviewStats
            .GroupBy(r => r.DocumentId)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    UsefulCount = g.FirstOrDefault(r => r.DocumentReviewType == DocumentReviewType.Useful)?.Count ?? 0,
                    NotUsefulCount = g.FirstOrDefault(r => r.DocumentReviewType == DocumentReviewType.NotUseful)?.Count ?? 0
                });

        var documentsData = await query
            .Where(d => documentIds.Contains(d.Id))
            .Select(d => new
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                Description = d.Description,
                Visibility = d.Visibility,
                Subject = d.Subject != null
                    ? new SubjectDto
                    {
                        Id = d.Subject.Id,
                        SubjectName = d.Subject.SubjectName,
                        SubjectCode = d.Subject.SubjectCode,
                        Majors = d.Subject.SubjectMajors.Select(sm => new MajorDto
                        {
                            Id = sm.Major.Id,
                            MajorName = sm.Major.MajorName,
                            MajorCode = sm.Major.MajorCode,
                            Faculty = sm.Major.Faculty != null
                                ? new FacultyDto
                                {
                                    Id = sm.Major.Faculty.Id,
                                    FacultyName = sm.Major.Faculty.FacultyName,
                                    FacultyCode = sm.Major.Faculty.FacultyCode,
                                    Logo = sm.Major.Faculty.Logo
                                }
                                : null
                        }).ToList()
                    }
                    : null,
                Type = new TypeDto
                {
                    Id = d.Type.Id,
                    TypeName = d.Type.TypeName
                },
                Tags = d.DocumentTags.Select(dt => new TagDto
                {
                    Id = dt.Tag.Id,
                    TagName = dt.Tag.TagName
                }).ToList(),
                Authors = d.DocumentAuthors
                    .Select(da => new AuthorDto
                    {
                        Id = da.Author.Id,
                        FullName = da.Author.FullName
                    })
                    .Distinct()
                    .ToList(),
                FileCount = d.DocumentFiles.Count,
                ThumbnailFileId = d.CoverFileId,
                CommentCount = d.DocumentFiles.SelectMany(df => df.Comments).Count(),
                TotalViewCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.ViewCount),
                CreatedById = d.CreatedById,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var documents = documentsData.Select(d => new DocumentDto
        {
            Id = d.Id,
            DocumentName = d.DocumentName,
            Description = d.Description,
            Visibility = d.Visibility,
            Subject = d.Subject,
            Type = d.Type,
            Tags = d.Tags,
            Authors = d.Authors,
            FileCount = d.FileCount,
            ThumbnailFileId = d.ThumbnailFileId,
            CommentCount = d.CommentCount,
            TotalViewCount = d.TotalViewCount,
            UsefulCount = reviewStatsDict.TryGetValue(d.Id, out var stats) ? stats.UsefulCount : 0,
            NotUsefulCount = reviewStatsDict.TryGetValue(d.Id, out var stats2) ? stats2.NotUsefulCount : 0,
            CreatedById = d.CreatedById,
            CreatedAt = d.CreatedAt
        }).ToList();

        return new PagedResponse<DocumentDto>
        {
            Items = documents,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}