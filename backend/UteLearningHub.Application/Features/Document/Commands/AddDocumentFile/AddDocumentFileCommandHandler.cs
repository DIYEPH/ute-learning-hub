using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainDocument = UteLearningHub.Domain.Entities.Document;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public class AddDocumentFileCommandHandler : IRequestHandler<AddDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<DocumentDetailDto> Handle(AddDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to modify documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        if (request.File == null || request.File.Length == 0)
            throw new BadRequestException("Chapter file is required");

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: false, cancellationToken);

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

        // Validate chapter file
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".doc", ".docx",
            ".pdf",
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
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
            throw new BadRequestException("Chapter file must be Word, PDF or image.");

        var uploadedFileUrls = new List<string>();

        try
        {
            // Upload chapter file
            using (var fileStream = request.File.OpenReadStream())
            {
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

                var chapter = new DocumentFile
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    FileId = file.Id,
                    Title = request.Title,
                    TotalPages = request.TotalPages,
                    IsPrimary = request.IsPrimary,
                    Order = request.Order ?? (document.DocumentFiles.Any()
                        ? document.DocumentFiles.Max(df => df.Order) + 1
                        : 1),
                    CreatedById = userId,
                    UpdatedById = null
                };

                document.DocumentFiles.Add(chapter);

                // Optional cover image for chapter
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
                        throw new BadRequestException("Chapter cover image must be an image file (jpg, png, gif, webp).");

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
                    chapter.CoverFileId = coverFile.Id;
                }
            }

            await _documentRepository.UpdateAsync(document, cancellationToken);
            await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var url in uploadedFileUrls)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(url, cancellationToken);
                }
                catch
                {
                }
            }

            throw;
        }

        // Reload with details to return updated document
        var updated = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: true, cancellationToken);

        if (updated == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found after update");

        var commentCount = await _documentRepository.GetQueryableSet()
            .Where(d => d.Id == request.DocumentId)
            .Select(d => d.DocumentFiles.SelectMany(df => df.Comments).Count())
            .FirstOrDefaultAsync(cancellationToken);

        var usefulCount = 0;
        var notUsefulCount = 0;
        var totalCount = 0;

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
            CreatedById = updated.CreatedById,
            CreatedAt = updated.CreatedAt,
            UpdatedAt = updated.UpdatedAt
        };
    }
}


