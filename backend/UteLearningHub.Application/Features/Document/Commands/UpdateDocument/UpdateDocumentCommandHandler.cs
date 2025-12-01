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
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDetailDto>
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

    public UpdateDocumentCommandHandler(
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

        var uploadedFileUrls = new List<string>();
        var filesToDeleteFromStorage = new List<string>(); 

        // Cập nhật ảnh bìa nếu có
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

            if (document.CoverFile != null)
            {
                filesToDeleteFromStorage.Add(document.CoverFile.FileUrl);
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

        document.UpdatedById = userId;
        document.UpdatedAt = _dateTimeProvider.OffsetNow;

        try
        {
            await _documentRepository.UpdateAsync(document, cancellationToken);
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

        foreach (var fileUrl in filesToDeleteFromStorage)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(fileUrl, cancellationToken);
            }
            catch
            {
            }
        }

        document = await _documentRepository.GetByIdWithDetailsAsync(request.Id, disableTracking: true, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.Id)
            .Select(d => d.DocumentFiles.SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

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
            CreatedById = document.CreatedById,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
