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
            query = query.Where(d => d.Subject.SubjectMajors.Any(sm => sm.MajorId == request.MajorId.Value));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(d =>
                d.DocumentName.ToLower().Contains(searchTerm) ||
                d.Description.ToLower().Contains(searchTerm) ||
                d.AuthorName.ToLower().Contains(searchTerm));
        }

        if (request.Visibility.HasValue)
            query = query.Where(d => d.Visibility == request.Visibility.Value);

        if (request.ReviewStatus.HasValue)
            query = query.Where(d => d.ReviewStatus == request.ReviewStatus.Value);

        if (request.IsDownload.HasValue)
            query = query.Where(d => d.IsDownload == request.IsDownload.Value);

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!request.ReviewStatus.HasValue && !isAdmin)
        {
            query = query.Where(d => d.ReviewStatus == ReviewStatus.Approved);
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(d => d.DocumentName)
                : query.OrderBy(d => d.DocumentName),
            "author" or "authorname" => request.SortDescending
                ? query.OrderByDescending(d => d.AuthorName)
                : query.OrderBy(d => d.AuthorName),
            "createdat" or "date" => request.SortDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt),
            _ => query.OrderByDescending(d => d.CreatedAt) // Default: newest first
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var documents = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                Description = d.Description,
                AuthorName = d.AuthorName,
                DescriptionAuthor = d.DescriptionAuthor,
                IsDownload = d.IsDownload,
                Visibility = d.Visibility,
                ReviewStatus = d.ReviewStatus,
                Subject = new SubjectDto
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
                            FacultyCode = sm.Major.Faculty.FacultyCode
                        } : null
                    }).ToList()
                },
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
                FileCount = d.DocumentFiles.Count,
                CommentCount = d.Comments.Count,
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
