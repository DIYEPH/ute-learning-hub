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
    private readonly ICurrentUserService _currentUserService;

    public GetDocumentsHandler(IDocumentRepository documentRepository, ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResponse<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var query = _documentRepository.GetQueryableWithIncludes()
            .AsNoTracking();

        if (request.SubjectId.HasValue)
            query = query.Where(d => d.SubjectId == request.SubjectId.Value);

        if (request.TypeId.HasValue)
            query = query.Where(d => d.TypeId == request.TypeId.Value);

        if (request.TagId.HasValue)
            query = query.Where(d => d.DocumentTags.Any(dt => dt.TagId == request.TagId.Value));

        if (request.MajorId.HasValue)
            query = query.Where(d => d.Subject != null && d.Subject.SubjectMajors.Any(sm => sm.MajorId == request.MajorId.Value));

        if (request.AuthorId.HasValue)
            query = query.Where(d => d.DocumentAuthors.Any(da => da.AuthorId == request.AuthorId.Value));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(searchTerm) ||
                d.Description.ToLower().Contains(searchTerm));
        }

        if (request.Visibility.HasValue)
            query = query.Where(d => d.Visibility == request.Visibility.Value);

        if (request.ReviewStatus.HasValue)
            query = query.Where(d => d.ReviewStatus == request.ReviewStatus.Value);

        if (request.IsDownload.HasValue)
            query = query.Where(d => d.IsDownload == request.IsDownload.Value);

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        var isAuthenticated = _currentUserService.IsAuthenticated;

        if (!request.ReviewStatus.HasValue && !isAdmin)
        {
            query = query.Where(d => d.ReviewStatus == ReviewStatus.Approved);
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

        // Apply pagination
        // Chỉ hiển thị tài liệu đã có ít nhất 1 file chương
        query = query.Where(d => d.DocumentFiles.Any());

        var documents = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                Description = d.Description,
                IsDownload = d.IsDownload,
                Visibility = d.Visibility,
                ReviewStatus = d.ReviewStatus,
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
                ThumbnailUrl = d.CoverFile != null ? d.CoverFile.FileUrl : null,
                CommentCount = d.DocumentFiles.SelectMany(df => df.Comments).Count(),
                CreatedById = d.CreatedById,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<DocumentDto>
        {
            Items = documents,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
