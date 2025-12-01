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

                var existingTag = await _tagRepository.GetQueryableSet()
                    .Where(t => !t.IsDeleted && t.TagName != null)
                    .FirstOrDefaultAsync(
                        t => string.Equals(t.TagName, normalizedName, StringComparison.OrdinalIgnoreCase),
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

        if (request.File == null || request.File.Length == 0)
            throw new BadRequestException("Document must have exactly one file");

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx",        // Word
            ".pdf",                 // PDF
            ".jpg", ".jpeg", ".png", ".gif", ".webp" // Images
        };

        var allowedMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

        var fileExtension = Path.GetExtension(request.File.FileName);
        var fileMimeType = request.File.ContentType;

        if (!allowedExtensions.Contains(fileExtension) || !allowedMimeTypes.Contains(fileMimeType))
        {
            throw new BadRequestException("Only Word documents, images, and PDF files are allowed.");
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
            AuthorName = request.AuthorName ?? string.Empty,  
            DescriptionAuthor = request.DescriptionAuthor ?? string.Empty,  
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

        var uploadedFileUrls = new List<string>(); 

        try
        {
            var formFile = request.File;
            if (formFile.Length > 0)
            {
                using var fileStream = formFile.OpenReadStream();
                var fileUrl = await _fileStorageService.UploadFileAsync(
                    fileStream,
                    formFile.FileName,
                    formFile.ContentType,
                    cancellationToken);

                uploadedFileUrls.Add(fileUrl); 

                var file = new DomainFile
                {
                    Id = Guid.NewGuid(),
                    FileName = formFile.FileName,
                    FileUrl = fileUrl,
                    FileSize = formFile.Length,
                    MimeType = formFile.ContentType,
                    CreatedById = userId,
                    CreatedAt = _dateTimeProvider.OffsetNow
                };

                await _fileRepository.AddAsync(file, cancellationToken);

                document.FileId = file.Id;
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
            .Select(d => d.Comments.Count)
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
            AuthorName = document.AuthorName,
            DescriptionAuthor = document.DescriptionAuthor,
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
            File = document.File == null ? null : new DocumentFileDto
            {
                Id = document.File.Id,
                FileName = document.File.FileName,
                FileUrl = document.File.FileUrl,
                FileSize = document.File.FileSize,
                MimeType = document.File.MimeType
            },
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
