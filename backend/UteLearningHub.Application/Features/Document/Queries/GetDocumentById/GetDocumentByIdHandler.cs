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
        var isAuthenticated = _currentUserService.IsAuthenticated;

        // Chỉ admin mới xem được tài liệu chưa duyệt
        if (!isAdmin && document.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Người chưa đăng nhập chỉ xem được tài liệu Public
        if (!isAuthenticated && document.Visibility != VisibilityStatus.Public)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Tổng số comment của toàn bộ document (tất cả DocumentFile)
        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .Select(d => d.DocumentFiles.SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

        // Thống kê review cho toàn bộ document
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

        // Thống kê review theo từng DocumentFile
        var perFileReviewStats = await _documentReviewRepository.GetQueryableSet()
            .Where(dr => dr.DocumentId == request.Id && !dr.IsDeleted && dr.DocumentFileId != Guid.Empty)
            .GroupBy(dr => dr.DocumentFileId)
            .Select(g => new
            {
                DocumentFileId = g.Key,
                Useful = g.Count(r => r.DocumentReviewType == DocumentReviewType.Useful),
                NotUseful = g.Count(r => r.DocumentReviewType == DocumentReviewType.NotUseful)
            })
            .ToListAsync(cancellationToken);

        var perFileReviewDict = perFileReviewStats.ToDictionary(
            x => x.DocumentFileId,
            x => (x.Useful, x.NotUseful)
        );

        // Thống kê comment theo từng DocumentFile
        var perFileCommentStats = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .SelectMany(d => d.DocumentFiles)
            .Select(df => new
            {
                DocumentFileId = df.Id,
                CommentCount = df.Comments.Count(c => !c.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var perFileCommentDict = perFileCommentStats.ToDictionary(
            x => x.DocumentFileId,
            x => x.CommentCount
        );

        return new DocumentDetailDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            Description = document.Description,
            IsDownload = document.IsDownload,
            Visibility = document.Visibility,
            ReviewStatus = document.ReviewStatus,
            Subject = document.Subject != null ? new SubjectDto
            {
                Id = document.Subject.Id,
                SubjectName = document.Subject.SubjectName,
                SubjectCode = document.Subject.SubjectCode,
                Majors = document.Subject.SubjectMajors.Select(sm => new MajorDto
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
                Id = document.Type.Id,
                TypeName = document.Type.TypeName
            },
            Tags = document.DocumentTags.Select(dt => new TagDto
            {
                Id = dt.Tag.Id,
                TagName = dt.Tag.TagName
            }).ToList(),
            Authors = document.DocumentAuthors
                .Select(da => new AuthorDto
                {
                    Id = da.Author.Id,
                    FullName = da.Author.FullName
                })
                .Distinct()
                .ToList(),
            CoverUrl = document.CoverFile != null ? document.CoverFile.FileUrl : null,
            // Danh sách các file/chương (DocumentFiles)
            Files = document.DocumentFiles
                .OrderBy(df => df.Order)
                .ThenBy(df => df.CreatedAt)
                .Select(df =>
                {
                    perFileReviewDict.TryGetValue(df.Id, out var reviewForFile);
                    perFileCommentDict.TryGetValue(df.Id, out var commentCountForFile);

                    var usefulForFile = reviewForFile.Useful;
                    var notUsefulForFile = reviewForFile.NotUseful;

                    return new DocumentFileDto
                    {
                        Id = df.Id,
                        FileName = df.File.FileName,
                        FileUrl = df.File.FileUrl,
                        FileSize = df.File.FileSize,
                        MimeType = df.File.MimeType,
                        Title = df.Title,
                        Order = df.Order,
                        IsPrimary = df.IsPrimary,
                        TotalPages = df.TotalPages,
                        CoverUrl = df.CoverFile != null ? df.CoverFile.FileUrl : null,
                        CommentCount = commentCountForFile,
                        UsefulCount = usefulForFile,
                        NotUsefulCount = notUsefulForFile
                    };
                })
                .ToList(),
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
