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
    private readonly IUserDocumentProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;
    
    public GetDocumentByIdHandler(
        IDocumentRepository documentRepository,
        IDocumentReviewRepository documentReviewRepository,
        IUserDocumentProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _documentReviewRepository = documentReviewRepository;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DocumentDetailDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        var isAuthenticated = _currentUserService.IsAuthenticated;

        // Load document với projection trực tiếp (chỉ lấy dữ liệu cần thiết)
        var document = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id && !d.IsDeleted)
            .Select(d => new
            {
                d.Id,
                d.DocumentName,
                d.Description,
                d.IsDownload,
                d.Visibility,
                d.ReviewStatus,
                d.CreatedById,
                d.CreatedAt,
                d.UpdatedAt,
                Subject = d.Subject == null ? null : new SubjectDto
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
                },
                Type = d.Type == null ? null : new TypeDto
                {
                    Id = d.Type.Id,
                    TypeName = d.Type.TypeName
                },
                Tags = d.DocumentTags
                    .Where(dt => dt.Tag != null && !dt.Tag.IsDeleted)
                    .Select(dt => new TagDto
                    {
                        Id = dt.Tag.Id,
                        TagName = dt.Tag.TagName
                    })
                    .ToList(),
                Authors = d.DocumentAuthors
                    .Where(da => da.Author != null && !da.Author.IsDeleted)
                    .Select(da => new AuthorDto
                    {
                        Id = da.Author.Id,
                        FullName = da.Author.FullName
                    })
                    .Distinct()
                    .ToList(),
                CoverFileId = d.CoverFileId,
                Files = d.DocumentFiles
                    .Where(df => !df.IsDeleted)
                    .OrderBy(df => df.Order)
                    .ThenBy(df => df.CreatedAt)
                    .Select(df => new
                    {
                        df.Id,
                        df.FileId,
                    df.File.FileName,
                    df.File.FileSize,
                    df.File.MimeType,
                        df.Title,
                        df.Order,
                        df.IsPrimary,
                        df.TotalPages,
                        df.CoverFileId
                    })
                    .ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Chỉ admin mới xem được tài liệu chưa duyệt
        if (!isAdmin && document.ReviewStatus != ReviewStatus.Approved)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Người chưa đăng nhập chỉ xem được tài liệu Public
        if (!isAuthenticated && document.Visibility != VisibilityStatus.Public)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Tổng số comment của toàn bộ document (tất cả DocumentFile)
        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .Select(d => d.DocumentFiles.Where(df => !df.IsDeleted).SelectMany(df => df.Comments).Count())
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
            .SelectMany(d => d.DocumentFiles.Where(df => !df.IsDeleted))
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

        // Load progress cho user hiện tại (nếu đã authenticated)
        Dictionary<Guid, DocumentProgressDto>? progressDict = null;
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            var progressList = await _progressRepository.GetByUserAndDocumentAsync(
                _currentUserService.UserId.Value,
                request.Id,
                disableTracking: true,
                cancellationToken);

            progressDict = progressList.ToDictionary(
                p => p.DocumentFileId ?? Guid.Empty,
                p => new DocumentProgressDto
                {
                    DocumentFileId = p.DocumentFileId ?? Guid.Empty,
                    LastPage = p.LastPage,
                    TotalPages = p.TotalPages,
                    LastAccessedAt = p.LastAccessedAt
                }
            );
        }

        return new DocumentDetailDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            Description = document.Description,
            IsDownload = document.IsDownload,
            Visibility = document.Visibility,
            ReviewStatus = document.ReviewStatus,
            Subject = document.Subject,
            Type = document.Type ?? new TypeDto { Id = Guid.Empty, TypeName = string.Empty },
            Tags = document.Tags,
            Authors = document.Authors,
            CoverFileId = document.CoverFileId,
            Files = document.Files
                .Select(df =>
                {
                    perFileReviewDict.TryGetValue(df.Id, out var reviewForFile);
                    perFileCommentDict.TryGetValue(df.Id, out var commentCountForFile);

                    var usefulForFile = reviewForFile.Useful;
                    var notUsefulForFile = reviewForFile.NotUseful;

                    DocumentProgressDto? progress = null;
                    if (progressDict != null)
                    {
                        progressDict.TryGetValue(df.Id, out progress);
                    }

                    return new DocumentFileDto
                    {
                        Id = df.Id,
                        FileId = df.FileId,
                        FileSize = df.FileSize,
                        MimeType = df.MimeType,
                        Title = df.Title,
                        Order = df.Order,
                        IsPrimary = df.IsPrimary,
                        TotalPages = df.TotalPages,
                        CoverFileId = df.CoverFileId,
                        CommentCount = commentCountForFile,
                        UsefulCount = usefulForFile,
                        NotUsefulCount = notUsefulForFile,
                        Progress = progress
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
