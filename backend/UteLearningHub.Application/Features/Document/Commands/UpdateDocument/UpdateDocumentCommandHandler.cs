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

        // Check permission: only owner, admin, or moderator can update
        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner || 
                       isAdmin || 
                       (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to update this document");

        // Update properties if provided
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

        // Update tags if provided
        if (request.TagIds != null)
        {
            // Remove existing tags
            document.DocumentTags.Clear();

            // Validate and add new tags
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

        // Handle file removal
        var filesToDeleteFromStorage = new List<string>(); // Track files to delete from storage
        if (request.FileIdsToRemove != null && request.FileIdsToRemove.Any())
        {
            // Validate that files belong to this document
            var existingFileIds = document.DocumentFiles.Select(df => df.FileId).ToList();
            var invalidFileIds = request.FileIdsToRemove.Where(fid => !existingFileIds.Contains(fid)).ToList();
            
            if (invalidFileIds.Any())
                throw new NotFoundException($"One or more files not found in this document: {string.Join(", ", invalidFileIds)}");

            // Get files to delete
            var filesToRemove = await _fileRepository.GetQueryableSet()
                .Where(f => request.FileIdsToRemove.Contains(f.Id) && !f.IsDeleted)
                .ToListAsync(cancellationToken);

            // Remove DocumentFile relationships
            var documentFilesToRemove = document.DocumentFiles
                .Where(df => request.FileIdsToRemove.Contains(df.FileId))
                .ToList();

            foreach (var docFile in documentFilesToRemove)
            {
                document.DocumentFiles.Remove(docFile);
                filesToDeleteFromStorage.Add(filesToRemove.First(f => f.Id == docFile.FileId).FileUrl);
            }

            // Delete File entities (soft delete)
            foreach (var file in filesToRemove)
            {
                file.IsDeleted = true;
                file.DeletedAt = _dateTimeProvider.OffsetNow;
                file.DeletedById = userId;
            }
        }

        // Handle file addition
        var uploadedFileUrls = new List<string>(); // Track uploaded files for rollback
        if (request.FilesToAdd != null && request.FilesToAdd.Any())
        {
            // Calculate current file count after removal
            var currentFileCount = document.DocumentFiles.Count;
            if (request.FileIdsToRemove != null && request.FileIdsToRemove.Any())
            {
                currentFileCount -= request.FileIdsToRemove.Count;
            }

            // Validate total file count - maximum 3 files
            var totalFileCount = currentFileCount + request.FilesToAdd.Count;
            if (totalFileCount > 3)
                throw new BadRequestException("Maximum 3 files are allowed per document. Current files: " + currentFileCount + ", trying to add: " + request.FilesToAdd.Count);

            foreach (var formFile in request.FilesToAdd)
            {
                if (formFile.Length > 0)
                {
                    // Upload file to storage
                    using var fileStream = formFile.OpenReadStream();
                    var fileUrl = await _fileStorageService.UploadFileAsync(
                        fileStream,
                        formFile.FileName,
                        formFile.ContentType,
                        cancellationToken);

                    uploadedFileUrls.Add(fileUrl); // Track for rollback

                    // Create File entity
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

                    // Create DocumentFile relationship
                    document.DocumentFiles.Add(new DocumentFile
                    {
                        DocumentId = document.Id,
                        FileId = file.Id
                    });
                }
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
            // Rollback: Delete uploaded files from storage
            foreach (var fileUrl in uploadedFileUrls)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(fileUrl, cancellationToken);
                }
                catch
                {
                    // Log error but continue cleanup
                }
            }
            throw; // Re-throw original exception
        }

        // Delete files from storage after successful DB save
        foreach (var fileUrl in filesToDeleteFromStorage)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(fileUrl, cancellationToken);
            }
            catch
            {
                // Log error but continue (file might already be deleted)
            }
        }

        // Reload to get updated relationships
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
            Files = document.DocumentFiles.Select(df => new DocumentFileDto
            {
                Id = df.File.Id,
                FileName = df.File.FileName,
                FileUrl = df.File.FileUrl,
                FileSize = df.File.FileSize,
                MimeType = df.File.MimeType
            }).ToList(),
            CommentCount = commentCount,
            CreatedById = document.CreatedById,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
