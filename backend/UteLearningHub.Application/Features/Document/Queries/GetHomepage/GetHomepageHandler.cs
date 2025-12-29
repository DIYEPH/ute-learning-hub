using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetHomepage;

public class GetHomepageHandler : IRequestHandler<GetHomepageQuery, HomepageDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetHomepageHandler(
        IDocumentRepository documentRepository,
        IDocumentReviewRepository documentReviewRepository,
        ISubjectRepository subjectRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _documentReviewRepository = documentReviewRepository;
        _subjectRepository = subjectRepository;
        _currentUserService = currentUserService;
    }

    public async Task<HomepageDto> Handle(GetHomepageQuery request, CancellationToken cancellationToken)
    {
        var isAuthenticated = _currentUserService.IsAuthenticated;

        // Base query - only public docs for non-authenticated, public + internal for authenticated
        var baseQuery = _documentRepository.GetQueryableWithIncludes()
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Where(d => d.DocumentFiles.Any(f => !f.IsDeleted && f.Status == ContentStatus.Approved));

        if (!isAuthenticated)
        {
            baseQuery = baseQuery.Where(d => d.Visibility == VisibilityStatus.Public);
        }
        else
        {
            baseQuery = baseQuery.Where(d =>
                d.Visibility == VisibilityStatus.Public ||
                d.Visibility == VisibilityStatus.Internal);
        }

        // 1. Latest 12 documents
        var latestDocs = await baseQuery
            .OrderByDescending(d => d.CreatedAt)
            .Take(12)
            .Select(d => MapToDocumentDto(d))
            .ToListAsync(cancellationToken);

        // 2. Popular 12 documents (by useful count)
        var popularDocIds = await _documentReviewRepository.GetQueryableSet()
            .Where(r => r.DocumentReviewType == DocumentReviewType.Useful)
            .GroupBy(r => r.DocumentId)
            .OrderByDescending(g => g.Count())
            .Take(12)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        var popularDocs = await baseQuery
            .Where(d => popularDocIds.Contains(d.Id))
            .Select(d => MapToDocumentDto(d))
            .ToListAsync(cancellationToken);

        // Sort by the order in popularDocIds
        popularDocs = popularDocIds
            .Select(id => popularDocs.FirstOrDefault(d => d.Id == id))
            .Where(d => d != null)
            .ToList()!;

        // 3. Most viewed 12 documents (by total view count of all files)
        var mostViewedDocs = await baseQuery
            .OrderByDescending(d => d.DocumentFiles.Where(f => !f.IsDeleted).Sum(f => f.ViewCount))
            .Take(12)
            .Select(d => MapToDocumentDto(d))
            .ToListAsync(cancellationToken);

        // 4. Top 5 subjects by document count
        var topSubjectIds = await baseQuery
            .Where(d => d.SubjectId != null)
            .GroupBy(d => d.SubjectId)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key!.Value)
            .ToListAsync(cancellationToken);

        var topSubjects = new List<SubjectWithDocsDto>();

        foreach (var subjectId in topSubjectIds)
        {
            var subjectInfo = await _subjectRepository.GetQueryableSet()
                .AsNoTracking()
                .Where(s => s.Id == subjectId)
                .Select(s => new { s.Id, s.SubjectName, s.SubjectCode })
                .FirstOrDefaultAsync(cancellationToken);

            if (subjectInfo == null) continue;

            var docs = await baseQuery
                .Where(d => d.SubjectId == subjectId)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .Select(d => MapToDocumentDto(d))
                .ToListAsync(cancellationToken);

            topSubjects.Add(new SubjectWithDocsDto
            {
                SubjectId = subjectInfo.Id,
                SubjectName = subjectInfo.SubjectName,
                SubjectCode = subjectInfo.SubjectCode,
                Documents = docs
            });
        }

        return new HomepageDto
        {
            LatestDocuments = latestDocs,
            PopularDocuments = popularDocs,
            MostViewedDocuments = mostViewedDocs,
            TopSubjects = topSubjects
        };
    }

    private static DocumentDto MapToDocumentDto(Domain.Entities.Document d) => new()
    {
        Id = d.Id,
        DocumentName = d.DocumentName,
        Description = d.Description,
        Visibility = d.Visibility,
        Subject = d.Subject != null ? new SubjectDto
        {
            Id = d.Subject.Id,
            SubjectName = d.Subject.SubjectName,
            SubjectCode = d.Subject.SubjectCode
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
        Authors = d.DocumentAuthors.Select(da => new AuthorDetailDto
        {
            Id = da.Author.Id,
            FullName = da.Author.FullName
        }).ToList(),
        FileCount = d.DocumentFiles.Count(f => !f.IsDeleted),
        ThumbnailFileId = d.CoverFileId,
        CommentCount = d.DocumentFiles.Where(f => !f.IsDeleted).Sum(f => f.Comments.Count(c => !c.IsDeleted)),
        TotalViewCount = d.DocumentFiles.Where(f => !f.IsDeleted).Sum(f => f.ViewCount),
        UsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
        NotUsefulCount = d.Reviews.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful),
        CreatedById = d.CreatedById,
        CreatedAt = d.CreatedAt
    };
}
