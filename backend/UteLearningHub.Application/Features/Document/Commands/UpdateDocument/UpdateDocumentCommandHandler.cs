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

        if (!string.IsNullOrWhiteSpace(request.AuthorName))
            document.AuthorName = request.AuthorName;

        if (!string.IsNullOrWhiteSpace(request.DescriptionAuthor))
            document.DescriptionAuthor = request.DescriptionAuthor;

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

        if (request.File != null)
        {
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

            var extension = Path.GetExtension(request.File.FileName);
            var mimeType = request.File.ContentType;

            if (!allowedExtensions.Contains(extension) || !allowedMimeTypes.Contains(mimeType))
            {
                throw new BadRequestException("Only Word documents, images, and PDF files are allowed.");
            }

            if (document.File != null)
            {
                filesToDeleteFromStorage.Add(document.File.FileUrl);
            }

            if (request.File.Length > 0)
            {
                using var fileStream = request.File.OpenReadStream();
                var fileUrl = await _fileStorageService.UploadFileAsync(
                    fileStream,
                    request.File.FileName,
                    request.File.ContentType,
                    cancellationToken);

                uploadedFileUrls.Add(fileUrl); 

                var file = new DomainFile
                {
                    Id = Guid.NewGuid(),
                    FileName = request.File.FileName,
                    FileUrl = fileUrl,
                    FileSize = request.File.Length,
                    MimeType = request.File.ContentType,
                    CreatedById = userId,
                    CreatedAt = _dateTimeProvider.OffsetNow
                };

                await _fileRepository.AddAsync(file, cancellationToken);

                document.FileId = file.Id;
            }
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
            .Select(d => d.Comments.Count)
            .FirstOrDefaultAsync(cancellationToken);

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
            CreatedById = document.CreatedById,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
