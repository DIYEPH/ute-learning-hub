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
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDocumentReviewRepository _documentReviewRepository;

    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        ISubjectRepository subjectRepository,
        ITypeRepository typeRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IDocumentReviewRepository documentReviewRepository)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _subjectRepository = subjectRepository;
        _typeRepository = typeRepository;
        _tagRepository = tagRepository;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _documentReviewRepository = documentReviewRepository;
    }

    public async Task<DocumentDetailDto> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.Id, disableTracking: false, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner || 
                       isAdmin || 
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to update this document");

        if (!string.IsNullOrWhiteSpace(request.DocumentName))
        {
            document.DocumentName = request.DocumentName;
            document.NormalizedName = request.DocumentName.ToLowerInvariant();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
            document.Description = request.Description;

        if (request.SubjectId.HasValue)
        {
            var subject = await _subjectRepository.GetByIdAsync(request.SubjectId.Value, disableTracking: true, cancellationToken);
            if (subject == null || subject.IsDeleted)
                throw new NotFoundException($"Subject with id {request.SubjectId.Value} not found");
            document.SubjectId = request.SubjectId.Value;
        }

        if (request.TypeId.HasValue)
        {
            var type = await _typeRepository.GetByIdAsync(request.TypeId.Value, disableTracking: true, cancellationToken);
            if (type == null || type.IsDeleted)
                throw new NotFoundException($"Type with id {request.TypeId.Value} not found");
            document.TypeId = request.TypeId.Value;
        }

        if (request.IsDownload.HasValue)
            document.IsDownload = request.IsDownload.Value;

        if (request.Visibility.HasValue)
            document.Visibility = request.Visibility.Value;
            
        if (request.TagIds != null)
        {
            document.DocumentTags.Clear();

            if (request.TagIds.Any())
            {
                var tags = await _tagRepository.GetQueryableSet()
                    .Where(t => request.TagIds.Contains(t.Id) && !t.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (tags.Count != request.TagIds.Count)
                    throw new NotFoundException("One or more tags not found");

                foreach (var tag in tags)
                {
                    document.DocumentTags.Add(new DocumentTag
                    {
                        DocumentId = document.Id,
                        TagId = tag.Id
                    });
                }
            }
        }

        DomainFile? fileToDelete = null;

        if (request.CoverFileId.HasValue && request.CoverFileId.Value == Guid.Empty)
            throw new BadRequestException("CoverFileId is invalid");

        if (request.CoverFileId.HasValue)
        {
            if (!document.CoverFileId.HasValue || document.CoverFileId.Value != request.CoverFileId.Value)
            {
                var newCoverFile = await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);

                if (document.CoverFileId.HasValue)
                {
                    fileToDelete = document.CoverFile ?? await _fileUsageService.EnsureFileAsync(document.CoverFileId.Value, cancellationToken);
                }

                document.CoverFileId = newCoverFile.Id;
            }
        }
        else if (document.CoverFileId.HasValue)
        {
            fileToDelete = document.CoverFile ?? await _fileUsageService.EnsureFileAsync(document.CoverFileId.Value, cancellationToken);
            document.CoverFileId = null;
        }

        document.UpdatedById = userId;
        document.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (fileToDelete != null)
            await _fileUsageService.DeleteFileAsync(fileToDelete, cancellationToken);

        document = await _documentRepository.GetByIdWithDetailsAsync(request.Id, disableTracking: true, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .Select(d => d.DocumentFiles.Where(df => !df.IsDeleted).SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

        // Thống kê review tổng cho document
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
                SubjectCode = document.Subject.SubjectCode
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
            Files = document.DocumentFiles
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
            CreatedById = document.CreatedById,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
