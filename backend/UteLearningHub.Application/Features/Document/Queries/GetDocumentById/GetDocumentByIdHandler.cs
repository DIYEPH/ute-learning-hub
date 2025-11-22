using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly ICurrentUserService _currentUserService;
    
    public GetDocumentByIdHandler(
        IDocumentRepository documentRepository,
        IDocumentReviewRepository documentReviewRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _documentReviewRepository = documentReviewRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DocumentDetailDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdWithDetailsAsync(request.Id, disableTracking: true, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        if (!isAdmin && document.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .Select(d => d.Comments.Count)
            .FirstOrDefaultAsync(cancellationToken);

        // Get review stats
        var reviewStats = await _documentReviewRepository.GetQueryableSet()
            .Where(dr => dr.DocumentId == request.Id && !dr.IsDeleted)
            .GroupBy(dr => dr.DocumentReviewType)
            .Select(g => new
            {
                DocumentReviewType = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var usefulCount = reviewStats.FirstOrDefault(r => r.DocumentReviewType == DocumentReviewType.Useful)?.Count ?? 0;
        var notUsefulCount = reviewStats.FirstOrDefault(r => r.DocumentReviewType == DocumentReviewType.NotUseful)?.Count ?? 0;
        var totalCount = reviewStats.Sum(r => r.Count);

        return new DocumentDetailDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            Description = document.Description,
            AuthorName = document.AuthorName,
            DescriptionAuthor = document.DescriptionAuthor,
            IsDownload = document.IsDownload,
            Visibility = document.Visibility,
            ReviewStatus = document.ReviewStatus,
            Subject = new SubjectDto
            {
                Id = document.Subject.Id,
                SubjectName = document.Subject.SubjectName,
                SubjectCode = document.Subject.SubjectCode
            },
            Type = new TypeDto
            {
                Id = document.Type.Id,
                TypeName = document.Type.TypeName
            },
            Tags = document.DocumentTags.Select(dt => new TagDto
            {
                Id = dt.Tag.Id,
                TagName = dt.Tag.TagName
            }).ToList(),
            Files = document.DocumentFiles.Select(df => new DocumentFileDto
            {
                Id = df.File.Id,
                FileName = df.File.FileName,
                FileUrl = df.File.FileUrl,
                FileSize = df.File.FileSize,
                MimeType = df.File.MimeType
            }).ToList(),
            CommentCount = commentCount,
            UsefulCount = usefulCount,
            NotUsefulCount = notUsefulCount,
            TotalCount = totalCount,
            CreatedById = document.CreatedById,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
