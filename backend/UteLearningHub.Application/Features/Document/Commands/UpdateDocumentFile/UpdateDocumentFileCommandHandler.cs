using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;

public class UpdateDocumentFileCommandHandler : IRequestHandler<UpdateDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDocumentReviewRepository _documentReviewRepository;

    public UpdateDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IDocumentReviewRepository documentReviewRepository)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _documentReviewRepository = documentReviewRepository;
    }

    public async Task<DocumentDetailDto> Handle(UpdateDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update document files");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: false, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner ||
                        isAdmin ||
                        (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to update this document file");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == request.DocumentFileId && !df.IsDeleted);

        if (fileEntity == null)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found in this document");

        // Update metadata
        if (!string.IsNullOrWhiteSpace(request.Title))
            fileEntity.Title = request.Title;

        if (request.Order.HasValue)
            fileEntity.Order = request.Order.Value;

        if (request.IsPrimary.HasValue)
            fileEntity.IsPrimary = request.IsPrimary.Value;

        Domain.Entities.File? oldCoverFile = null;

        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");

            if (!fileEntity.CoverFileId.HasValue || fileEntity.CoverFileId.Value != request.CoverFileId.Value)
            {
                var newCoverFile = await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);

                if (fileEntity.CoverFileId.HasValue)
                {
                    oldCoverFile = fileEntity.CoverFile ?? await _fileUsageService.EnsureFileAsync(fileEntity.CoverFileId.Value, cancellationToken);
                }

                fileEntity.CoverFileId = newCoverFile.Id;
            }
        }

        fileEntity.UpdatedById = userId;
        fileEntity.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (oldCoverFile != null)
        {
            await _fileUsageService.DeleteFileAsync(oldCoverFile, cancellationToken);
        }

        // Reload document for DTO (no tracking)
        var updated = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: true, cancellationToken);

        if (updated == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found after update");

        // Thống kê comment tổng
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


