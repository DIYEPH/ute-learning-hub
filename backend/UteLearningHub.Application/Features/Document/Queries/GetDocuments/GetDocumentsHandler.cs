using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocuments;

public class GetDocumentsHandler : IRequestHandler<GetDocumentsQuery, PagedResponse<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDocumentsHandler(
        IDocumentRepository documentRepository,
        IDocumentReviewRepository documentReviewRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _documentReviewRepository = documentReviewRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _documentRepository.GetQueryableWithIncludes()
            .AsNoTracking();

        // Filter by IsDeleted status (default: only active items)
        if (request.IsDeleted.HasValue)
            query = query.Where(d => d.IsDeleted == request.IsDeleted.Value);
        else
            query = query.Where(d => !d.IsDeleted);

        if (request.SubjectId.HasValue)
            query = query.Where(d => d.SubjectId == request.SubjectId.Value);

        if (request.TypeId.HasValue)
            query = query.Where(d => d.TypeId == request.TypeId.Value);

        if (request.TagIds != null && request.TagIds.Count > 0)
            query = query.Where(d => d.DocumentTags.Any(dt => request.TagIds.Contains(dt.TagId)));

        if (request.MajorId.HasValue)
            query = query.Where(d => d.Subject != null && d.Subject.SubjectMajors.Any(sm => sm.MajorId == request.MajorId.Value));

        if (request.AuthorId.HasValue)
            query = query.Where(d => d.DocumentAuthors.Any(da => da.AuthorId == request.AuthorId.Value));

        if (request.CreatedById.HasValue)
            query = query.Where(d => d.CreatedById == request.CreatedById.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(searchTerm) ||
                d.Description.ToLower().Contains(searchTerm));
        }

        if (request.Visibility.HasValue)
            query = query.Where(d => d.Visibility == request.Visibility.Value);

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        var isAuthenticated = _currentUserService.IsAuthenticated;

        // Non-admin users can only see documents with at least 1 approved file
        if (!isAdmin)
        {
            query = query.Where(d => d.DocumentFiles.Any(f => !f.IsDeleted && f.Status == ContentStatus.Approved));
        }

        if (!request.Visibility.HasValue)
        {
            if (!isAuthenticated)
            {
                query = query.Where(d => d.Visibility == VisibilityStatus.Public);
            }
            else
            {
                query = query.Where(d =>
                    d.Visibility == VisibilityStatus.Public ||
                    d.Visibility == VisibilityStatus.Internal);
            }
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(d => d.DocumentName)
                : query.OrderBy(d => d.DocumentName),
            "author" or "authorname" => request.SortDescending
                ? query.OrderByDescending(d => d.DocumentAuthors
                    .Select(da => da.Author.FullName)
                    .FirstOrDefault())
                : query.OrderBy(d => d.DocumentAuthors
                    .Select(da => da.Author.FullName)
                    .FirstOrDefault()),
            "createdat" or "date" => request.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt) // Default: newest first
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // Chỉ hiển thị tài liệu đã có ít nhất 1 file chương
        query = query.Where(d => d.DocumentFiles.Any());

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
                Subject = d.Subject != null ? new SubjectDto
                {
                    Id = d.Subject.Id,
                    SubjectName = d.Subject.SubjectName,
                    SubjectCode = d.Subject.SubjectCode,
                    Majors = d.Subject.SubjectMajors.Select(sm => new MajorDto
                    {
                        Id = sm.Major.Id,
                        MajorName = sm.Major.MajorName,
                        MajorCode = sm.Major.MajorCode,
                        Faculty = sm.Major.Faculty != null ? new FacultyDto
                        {
                            Id = sm.Major.Faculty.Id,
                            FacultyName = sm.Major.Faculty.FacultyName,
                            FacultyCode = sm.Major.Faculty.FacultyCode,
                            Logo = sm.Major.Faculty.Logo
                        } : null
                    }).ToList()
                } : null,
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
                TotalViewCount = d.DocumentFiles.Where(df => !df.IsDeleted).Sum(df => df.ViewCount),
                ThumbnailFileId = d.CoverFileId,
                CommentCount = d.DocumentFiles.SelectMany(df => df.Comments).Count(),
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
            TotalViewCount = d.TotalViewCount,
            ThumbnailFileId = d.ThumbnailFileId,
            CommentCount = d.CommentCount,
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
