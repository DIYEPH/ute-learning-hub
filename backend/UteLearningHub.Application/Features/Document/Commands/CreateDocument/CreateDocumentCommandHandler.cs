using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainDocument = UteLearningHub.Domain.Entities.Document;
using DomainTag = UteLearningHub.Domain.Entities.Tag;
using DomainFile = UteLearningHub.Domain.Entities.File;
using DomainAuthor = UteLearningHub.Domain.Entities.Author;


namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly IAuthorRepository _authorRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        IAuthorRepository authorRepository,
        ISubjectRepository subjectRepository,
        ITypeRepository typeRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _authorRepository = authorRepository;
        _subjectRepository = subjectRepository;
        _typeRepository = typeRepository;
        _tagRepository = tagRepository;
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

        // Xử lý AuthorIds - chọn authors có sẵn (giống TagIds)
        if (request.AuthorIds != null && request.AuthorIds.Any())
        {
            var existingAuthors = await _authorRepository.GetQueryableSet()
                .Where(a => request.AuthorIds.Contains(a.Id) && !a.IsDeleted)
                .ToListAsync(cancellationToken);

            if (existingAuthors.Count != request.AuthorIds.Count)
                throw new NotFoundException("One or more authors not found");

            authorIdsToAdd.AddRange(existingAuthors.Select(a => a.Id));
        }

        // Xử lý Authors - thêm authors mới (giống TagNames)
        if (request.Authors != null && request.Authors.Any())
        {
            foreach (var authorInput in request.Authors)
            {
                if (string.IsNullOrWhiteSpace(authorInput.FullName)) continue;

                var normalizedName = authorInput.FullName.Trim();
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
                    var newAuthor = new DomainAuthor
                    {
                        Id = Guid.NewGuid(),
                        FullName = normalizedName,
                        Description = authorInput.Description ?? string.Empty,
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

        // Add DocumentTags directly using tagIdsToAdd (no need to query DB again)
        foreach (var tagId in tagIdsToAdd)
        {
            document.DocumentTags.Add(new DocumentTag
            {
                DocumentId = document.Id,
                TagId = tagId
            });
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

        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid.");

            var coverFile = await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);
            document.CoverFileId = coverFile.Id;
        }

        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

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
                    Id = df.Id,
                    FileId = df.FileId,
                    FileSize = df.File.FileSize,
                    MimeType = df.File.MimeType,
                    Title = df.Title,
                    Order = df.Order,
                    IsPrimary = df.IsPrimary,
                    TotalPages = df.TotalPages,
                    CoverFileId = df.CoverFileId,
                    CommentCount = 0,
                    UsefulCount = 0,
                    NotUsefulCount = 0
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
