using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainDocument = UteLearningHub.Domain.Entities.Document;
using DomainFile = UteLearningHub.Domain.Entities.File;
using DomainTag = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IAuthorRepository _authorRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileRepository fileRepository,
        IAuthorRepository authorRepository,
        ISubjectRepository subjectRepository,
        ITypeRepository typeRepository,
        ITagRepository tagRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _fileRepository = fileRepository;
        _authorRepository = authorRepository;
        _subjectRepository = subjectRepository;
        _typeRepository = typeRepository;
        _tagRepository = tagRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DocumentDetailDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to create documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.DocumentName))
            throw new BadRequestException("DocumentName cannot be empty");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new BadRequestException("Description cannot be empty");

        if (request.SubjectId.HasValue)
        {
            var subject = await _subjectRepository.GetByIdAsync(request.SubjectId.Value, disableTracking: true, cancellationToken);
            if (subject == null || subject.IsDeleted)
                throw new NotFoundException($"Subject with id {request.SubjectId.Value} not found");
        }

        var type = await _typeRepository.GetByIdAsync(request.TypeId, disableTracking: true, cancellationToken);
        if (type == null || type.IsDeleted)
            throw new NotFoundException($"Type with id {request.TypeId} not found");

        var tagIdsToAdd = new List<Guid>();
        var authorIdsToAdd = new List<Guid>();

        if (request.TagIds != null && request.TagIds.Any())
        {
            var existingTags = await _tagRepository.GetQueryableSet()
                .Where(t => request.TagIds.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            if (existingTags.Count != request.TagIds.Count)
                throw new NotFoundException("One or more tags not found");

            tagIdsToAdd.AddRange(existingTags.Select(t => t.Id));
        }

        if (request.TagNames != null && request.TagNames.Any())
        {
            foreach (var tagName in request.TagNames)
            {
                if (string.IsNullOrWhiteSpace(tagName)) continue;

                var normalizedName = tagName.Trim();
                var normalizedNameLower = normalizedName.ToLowerInvariant();

                var existingTag = await _tagRepository.GetQueryableSet()
                    .Where(t => !t.IsDeleted && t.TagName != null)
                    .FirstOrDefaultAsync(
                        t => t.TagName!.ToLower() == normalizedNameLower,
                        cancellationToken);

                if (existingTag != null)
                {
                    if (!tagIdsToAdd.Contains(existingTag.Id))
                        tagIdsToAdd.Add(existingTag.Id);
                }
                else
                {
                    var titleCaseName = System.Globalization.CultureInfo
                        .CurrentCulture
                        .TextInfo
                        .ToTitleCase(normalizedName.ToLower());

                    var newTag = new DomainTag
                    {
                        Id = Guid.NewGuid(),
                        TagName = titleCaseName,
                        ReviewStatus = ReviewStatus.Approved, 
                        CreatedById = userId,
                        CreatedAt = _dateTimeProvider.OffsetNow
                    };

                    await _tagRepository.AddAsync(newTag, cancellationToken);
                    tagIdsToAdd.Add(newTag.Id);
                }
            }
        }

        if (!tagIdsToAdd.Any())
            throw new BadRequestException("Document must have at least one tag");

        // Xử lý danh sách tác giả (đa tác giả)
        if (request.AuthorNames != null && request.AuthorNames.Any())
        {
            foreach (var authorName in request.AuthorNames)
            {
                if (string.IsNullOrWhiteSpace(authorName)) continue;

                var normalizedName = authorName.Trim();
                var normalizedNameLower = normalizedName.ToLowerInvariant();

                var existingAuthor = await _authorRepository.GetQueryableSet()
                    .Where(a => !a.IsDeleted)
                    .FirstOrDefaultAsync(
                        a => a.FullName.ToLower() == normalizedNameLower,
                        cancellationToken);

                if (existingAuthor != null)
                {
                    if (!authorIdsToAdd.Contains(existingAuthor.Id))
                        authorIdsToAdd.Add(existingAuthor.Id);
                }
                else
                {
                    var newAuthor = new Author
                    {
                        Id = Guid.NewGuid(),
                        FullName = normalizedName,
                        Description = string.Empty,
                        ReviewStatus = ReviewStatus.Approved,
                        CreatedById = userId,
                        CreatedAt = _dateTimeProvider.OffsetNow
                    };

                    await _authorRepository.AddAsync(newAuthor, cancellationToken);
                    authorIdsToAdd.Add(newAuthor.Id);
                }
            }
        }

        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);
        var reviewStatus = (trustLevel.HasValue && trustLevel.Value >= TrustLever.Newbie)
            ? ReviewStatus.Approved
            : ReviewStatus.PendingReview;

        var document = new DomainDocument
        {
            Id = Guid.NewGuid(),
            DocumentName = request.DocumentName,
            NormalizedName = request.DocumentName.ToLowerInvariant(),
            Description = request.Description,
            SubjectId = request.SubjectId,  
            TypeId = request.TypeId,
            IsDownload = request.IsDownload,
            Visibility = request.Visibility,
            ReviewStatus = reviewStatus,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

        if (tagIdsToAdd.Any())
        {
            var tags = await _tagRepository.GetQueryableSet()
                .Where(t => tagIdsToAdd.Contains(t.Id) && !t.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var tag in tags)
            {
                document.DocumentTags.Add(new DocumentTag
                {
                    DocumentId = document.Id,
                    TagId = tag.Id
                });
            }
        }

        if (authorIdsToAdd.Any())
        {
            foreach (var authorId in authorIdsToAdd)
            {
                document.DocumentAuthors.Add(new DocumentAuthor
                {
                    DocumentId = document.Id,
                    AuthorId = authorId
                });
            }
        }

        var uploadedFileUrls = new List<string>(); 

        try
        {
            // Ảnh bìa tùy chọn
            if (request.CoverFile != null && request.CoverFile.Length > 0)
            {
                var coverExtension = Path.GetExtension(request.CoverFile.FileName);
                var coverMimeType = request.CoverFile.ContentType;

                var allowedCoverExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".webp"
                };

                var allowedCoverMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "image/jpeg",
                    "image/png",
                    "image/gif",
                    "image/webp"
                };

                if (!allowedCoverExtensions.Contains(coverExtension) || !allowedCoverMimeTypes.Contains(coverMimeType))
                {
                    throw new BadRequestException("Cover image must be an image file (jpg, png, gif, webp).");
                }

                using var coverStream = request.CoverFile.OpenReadStream();
                var coverUrl = await _fileStorageService.UploadFileAsync(
                    coverStream,
                    request.CoverFile.FileName,
                    request.CoverFile.ContentType,
                    cancellationToken);

                uploadedFileUrls.Add(coverUrl);

                var coverFile = new DomainFile
                {
                    Id = Guid.NewGuid(),
                    FileName = request.CoverFile.FileName,
                    FileUrl = coverUrl,
                    FileSize = request.CoverFile.Length,
                    MimeType = request.CoverFile.ContentType,
                    CreatedById = userId,
                    CreatedAt = _dateTimeProvider.OffsetNow
                };

                await _fileRepository.AddAsync(coverFile, cancellationToken);
                document.CoverFileId = coverFile.Id;
            }

            await _documentRepository.AddAsync(document, cancellationToken);
            await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var fileUrl in uploadedFileUrls)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(fileUrl, cancellationToken);
                }
                catch
                {
                }
            }
            throw; 
        }

        document = await _documentRepository.GetByIdWithDetailsAsync(document.Id, disableTracking: true, cancellationToken);

        if (document == null)
            throw new NotFoundException("Failed to create document");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == document.Id)
            .Select(d => d.DocumentFiles.SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

        var reviewStats = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == document.Id)
            .SelectMany(d => d.Reviews)
            .Where(r => !r.IsDeleted)
            .GroupBy(r => r.DocumentReviewType)
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
                .OrderBy(df => df.Order)
                .ThenBy(df => df.CreatedAt)
                .Select(df => new DocumentFileDto
                {
                    Id = df.File.Id,
                    FileName = df.File.FileName,
                    FileUrl = df.File.FileUrl,
                    FileSize = df.File.FileSize,
                    MimeType = df.File.MimeType,
                    Title = df.Title,
                    Order = df.Order,
                    IsPrimary = df.IsPrimary,
                    TotalPages = df.TotalPages,
                    CoverUrl = df.CoverFile != null ? df.CoverFile.FileUrl : null
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
