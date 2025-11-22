using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        ISubjectRepository subjectRepository,
        ITypeRepository typeRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _subjectRepository = subjectRepository;
        _typeRepository = typeRepository;
        _tagRepository = tagRepository;
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

        document.UpdatedById = userId;
        document.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _documentRepository.UpdateAsync(document, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

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
            Subject = new SubjectDto
            {
                Id = document.Subject.Id,
                SubjectName = document.Subject.SubjectName,
                SubjectCode = document.Subject.SubjectCode
            },
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
