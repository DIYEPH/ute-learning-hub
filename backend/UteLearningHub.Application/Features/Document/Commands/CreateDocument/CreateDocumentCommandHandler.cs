using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DomainAuthor = UteLearningHub.Domain.Entities.Author;
using DomainDocument = UteLearningHub.Domain.Entities.Document;
using DomainTag = UteLearningHub.Domain.Entities.Tag;

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
    private readonly IDocumentQueryService _documentQueryService;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        IAuthorRepository authorRepository,
        ISubjectRepository subjectRepository,
        ITypeRepository typeRepository,
        ITagRepository tagRepository,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IDocumentQueryService documentQueryService)
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
        _documentQueryService = documentQueryService;
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
            var existingTags = await _tagRepository.GetByIdsAsync(request.TagIds, cancellationToken: cancellationToken);

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
                var existingTag = await _tagRepository.FindByNameAsync(normalizedName, cancellationToken: cancellationToken);

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
                        Status = ContentStatus.Approved,
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

        if (request.AuthorIds != null && request.AuthorIds.Any())
        {
            var existingAuthors = await _authorRepository.GetByIdsAsync(request.AuthorIds, cancellationToken: cancellationToken);

            if (existingAuthors.Count != request.AuthorIds.Count)
                throw new NotFoundException("One or more authors not found");

            authorIdsToAdd.AddRange(existingAuthors.Select(a => a.Id));
        }

        if (request.Authors != null && request.Authors.Any())
        {
            foreach (var authorInput in request.Authors)
            {
                if (string.IsNullOrWhiteSpace(authorInput.FullName)) continue;

                var normalizedName = authorInput.FullName.Trim();
                var existingAuthor = await _authorRepository.FindByNameAsync(normalizedName, cancellationToken: cancellationToken);

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
                        Status = ContentStatus.Approved,
                        CreatedById = userId,
                        CreatedAt = _dateTimeProvider.OffsetNow
                    };

                    await _authorRepository.AddAsync(newAuthor, cancellationToken);
                    authorIdsToAdd.Add(newAuthor.Id);
                }
            }
        }

        var document = new DomainDocument
        {
            Id = Guid.NewGuid(),
            DocumentName = request.DocumentName,
            NormalizedName = request.DocumentName.ToLowerInvariant(),
            Description = request.Description,
            SubjectId = request.SubjectId,
            TypeId = request.TypeId,
            Visibility = request.Visibility,
            CreatedById = userId,
            CreatedAt = _dateTimeProvider.OffsetNow
        };

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

        var result = await _documentQueryService.GetDetailByIdAsync(document.Id, cancellationToken);
        return result ?? throw new NotFoundException("Failed to create document");
    }
}
