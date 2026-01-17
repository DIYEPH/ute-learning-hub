using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetMyDocuments;

public class GetMyDocumentsHandler(
    IDocumentRepository documentRepository,
    IDocumentReviewRepository documentReviewRepository,
    ICurrentUserService currentUserService) : IRequestHandler<GetMyDocumentsQuery, PagedResponse<DocumentDto>>
{
    public async Task<PagedResponse<DocumentDto>> Handle(GetMyDocumentsQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view your documents");

        var userId = currentUserService.UserId ?? throw new UnauthorizedException("User ID not found");

        var query = documentRepository.GetQueryableWithIncludes()
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

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(d => d.DocumentName)
                : query.OrderBy(d => d.DocumentName),
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

        // Thống kê review
        var reviewStats = await documentReviewRepository.GetQueryableSet()
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
                d.Id,
                d.DocumentName,
                d.Description,
                d.Visibility,
                Subject = d.Subject != null
                    ? new SubjectDto
                    {
                        Id = d.Subject.Id,
                        SubjectName = d.Subject.SubjectName,
                        SubjectCode = d.Subject.SubjectCode
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
                    .Select(da => new AuthorDetailDto
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
                d.CreatedById,
                d.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var documents = documentsData.Select(d =>
        {
            var stats = reviewStatsDict.GetValueOrDefault(d.Id);
            return new DocumentDto
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
                UsefulCount = stats?.UsefulCount ?? 0,
                NotUsefulCount = stats?.NotUsefulCount ?? 0,
                CreatedById = d.CreatedById,
                CreatedAt = d.CreatedAt
            };
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