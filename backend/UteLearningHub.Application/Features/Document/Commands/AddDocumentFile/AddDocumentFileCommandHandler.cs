using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.Recommendation;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public class AddDocumentFileCommandHandler : IRequestHandler<AddDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentPageCountService _documentPageCountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly IVectorMaintenanceService _vectorUpdateService;

    public AddDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IDocumentPageCountService documentPageCountService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDocumentReviewRepository documentReviewRepository,
        IVectorMaintenanceService vectorUpdateService)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _documentPageCountService = documentPageCountService;
        _currentUserService = currentUserService;
        _userService = userService;
        _documentReviewRepository = documentReviewRepository;
        _vectorUpdateService = vectorUpdateService;
    }

    public async Task<DocumentDetailDto> Handle(AddDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to modify documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: true, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner ||
                        isAdmin ||
                        (trustLevel.HasValue && trustLevel.Value >= Domain.Constaints.Enums.TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to add chapters to this document");

        if (request.FileId == Guid.Empty)
            throw new BadRequestException("FileId is required");

        // 1. Validate file chính tồn tại
        await _fileUsageService.EnsureFileAsync(request.FileId, cancellationToken);

        Guid? coverFileId = null;
        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");

            await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);
            coverFileId = request.CoverFileId.Value;
        }

        // 2. Tính Order từ document đã load (NoTracking)
        var nextOrder = document.DocumentFiles.Any()
            ? document.DocumentFiles.Max(df => df.Order) + 1
            : 1;

        // 3. Tự động detect TotalPages nếu chưa có
        int? totalPages = request.TotalPages;
        
        if (!totalPages.HasValue)
        {
            // Lấy file entity để biết MimeType và FileUrl
            var file = await _fileRepository.GetByIdAsync(request.FileId, disableTracking: true, cancellationToken);
            if (file != null)
            {
                try
                {
                    var fileStream = await _fileStorageService.GetFileAsync(file.FileUrl, cancellationToken);
                    if (fileStream != null)
                    {
                        totalPages = await _documentPageCountService.GetPageCountAsync(
                            fileStream, 
                            file.MimeType ?? "", 
                            cancellationToken);
                    }
                }
                catch
                {
                    // Nếu không đọc được, để null
                    totalPages = null;
                }
            }
        }

        // 4. Tạo DocumentFile mới và add qua repository
        var chapter = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            FileId = request.FileId,
            Title = request.Title,
            TotalPages = totalPages,
            IsPrimary = request.IsPrimary,
            Order = request.Order ?? nextOrder,
            CreatedById = userId,
            UpdatedById = null,
            CoverFileId = coverFileId
        };

        // Add qua repository method
        await _documentRepository.AddDocumentFileAsync(chapter, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Reload và map DTO (giữ nguyên phần dưới)
        var updated = await _documentRepository.GetByIdWithDetailsAsync(
            request.DocumentId, disableTracking: true, cancellationToken);

        if (updated == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found after update");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.DocumentId)
            .Select(d => d.DocumentFiles.Where(df => !df.IsDeleted).SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

        // Thống kê review tổng cho document
        var reviewStats = await _documentReviewRepository.GetQueryableSet()
            .Where(dr => dr.DocumentId == request.DocumentId && !dr.IsDeleted)
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
            .Where(dr => dr.DocumentId == request.DocumentId && !dr.IsDeleted && dr.DocumentFileId != Guid.Empty)
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
            .Where(d => d.Id == request.DocumentId)
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

        // Update user vector after adding document file (for behavior-based recommendations)
        await _vectorUpdateService.UpdateUserVectorAsync(userId, cancellationToken);

        return new DocumentDetailDto
        {
            Id = updated.Id,
            DocumentName = updated.DocumentName,
            Description = updated.Description,
            IsDownload = updated.IsDownload,
            Visibility = updated.Visibility,
            ReviewStatus = updated.ReviewStatus,
            Subject = updated.Subject != null ? new SubjectDto
            {
                Id = updated.Subject.Id,
                SubjectName = updated.Subject.SubjectName,
                SubjectCode = updated.Subject.SubjectCode,
                Majors = updated.Subject.SubjectMajors.Select(sm => new MajorDto
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
                Id = updated.Type.Id,
                TypeName = updated.Type.TypeName
            },
            Tags = updated.DocumentTags.Select(dt => new TagDto
            {
                Id = dt.Tag.Id,
                TagName = dt.Tag.TagName
            }).ToList(),
            Authors = updated.DocumentAuthors
                .Select(da => new AuthorDto
                {
                    Id = da.Author.Id,
                    FullName = da.Author.FullName
                })
                .Distinct()
                .ToList(),
            Files = updated.DocumentFiles
                .Where(df => !df.IsDeleted)
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
                        FileId = df.FileId,
                        FileSize = df.File.FileSize,
                        MimeType = df.File.MimeType,
                        Title = df.Title,
                        Order = df.Order,
                        IsPrimary = df.IsPrimary,
                        TotalPages = df.TotalPages,
                        CoverFileId = df.CoverFileId,
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
            CreatedById = updated.CreatedById,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }
}