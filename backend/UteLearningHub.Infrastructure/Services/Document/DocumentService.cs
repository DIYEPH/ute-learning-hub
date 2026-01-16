using System.Globalization;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.CreateDocument;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Features.Document.Queries.GetDocuments;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using AuthorEntity = UteLearningHub.Domain.Entities.Author;
using DocumentEntity = UteLearningHub.Domain.Entities.Document;
using TagEntity = UteLearningHub.Domain.Entities.Tag;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentService(
    IDocumentRepository documentRepository,
    IUserDocumentProgressRepository progressRepository,
    IFileUsageService fileUsageService,
    IAuthorRepository authorRepository,
    ISubjectRepository subjectRepository,
    ITypeRepository typeRepository,
    ITagRepository tagRepository,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IDocumentQueryService queryService,
    IUserService userService) : IDocumentService
{
    public async Task<DocumentDetailDto> CreateAsync(CreateDocumentCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        if (string.IsNullOrWhiteSpace(request.DocumentName))
            throw new BadRequestException("DocumentName cannot be empty");
        if (string.IsNullOrWhiteSpace(request.Description))
            throw new BadRequestException("Description cannot be empty");

        await ValidateSubjectAndTypeAsync(request.SubjectId, request.TypeId, ct);

        var tagIds = await ResolveTagIdsAsync(request.TagIds, request.TagNames, userId, ct);
        if (!tagIds.Any())
            throw new BadRequestException("Document must have at least one tag");

        var authorIds = await ResolveAuthorIdsAsync(request.AuthorIds, request.Authors, userId, ct);

        var document = new DocumentEntity
        {
            Id = Guid.NewGuid(),
            DocumentName = request.DocumentName,
            NormalizedName = request.DocumentName.ToLowerInvariant(),
            Description = request.Description,
            SubjectId = request.SubjectId,
            TypeId = request.TypeId,
            Visibility = request.Visibility,
            CreatedById = userId,
            CreatedAt = dateTimeProvider.OffsetNow
        };

        AttachTags(document, tagIds);
        AttachAuthors(document, authorIds);
        await AttachCoverFileAsync(document, request.CoverFileId, ct);

        documentRepository.Add(document);
        await documentRepository.UnitOfWork.SaveChangesAsync(ct);

        return await queryService.GetDetailByIdAsync(document.Id, ct)
            ?? throw new BadRequestException("Failed to create document");
    }

    public async Task<DocumentDetailDto> GetDocumentByIdAsync(Guid id, CancellationToken ct)
    {
        var isAdmin = currentUserService.IsInRole("Admin");
        var userId = currentUserService.UserId;

        var document = await queryService.GetDetailByIdAsync(id, ct)
            ?? throw new NotFoundException($"Document with id {id} not found");

        var isOwner = userId.HasValue && document.CreatedById == userId;
        var hasApprovedFile = document.Files.Any(f => f.Status == ContentStatus.Approved);

        if (!isAdmin && !isOwner && !hasApprovedFile)
            throw new NotFoundException($"Document with id {id} not found");
        if (!userId.HasValue && document.Visibility != VisibilityStatus.Public)
            throw new NotFoundException($"Document with id {id} not found");

        if (!userId.HasValue)
            return document;

        // Gắn progress cho từng file
        var progressList = await progressRepository.GetByUserAndDocumentAsync(userId.Value, id, true, ct);
        var progressDict = progressList
            .Where(p => p.DocumentFileId.HasValue)
            .ToDictionary(
                p => p.DocumentFileId!.Value,
                p => new DocumentFileProgressDto
                {
                    DocumentFileId = p.DocumentFileId!.Value,
                    LastPage = p.LastPage,
                    TotalPages = p.TotalPages,
                    LastAccessedAt = p.LastAccessedAt
                });

        return document with
        {
            Files = document.Files.Select(f =>
            {
                progressDict.TryGetValue(f.Id, out var progress);
                return f with { Progress = progress };
            }).ToList()
        };
    }

    public Task<PagedResponse<DocumentDetailDto>> GetDocumentsAsync(GetDocumentsQuery request, CancellationToken ct)
        => throw new NotImplementedException();

    public async Task SoftDeleteAsync(Guid documentId, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var document = await documentRepository.GetByIdAsync(documentId, cancellationToken: ct)
            ?? throw new NotFoundException($"Document with id {documentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);
        var canDelete = isOwner || isAdmin || (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this document");

        document.IsDeleted = true;
        document.DeletedAt = dateTimeProvider.OffsetUtcNow;
        document.DeletedById = userId;

        await documentRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task<DocumentDetailDto> UpdateAsync(UpdateDocumentCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var document = await documentRepository.GetByIdForUpdateAsync(request.Id, ct)
            ?? throw new NotFoundException($"Document with id {request.Id} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);
        var canUpdate = isOwner || isAdmin || (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to update this document");

        if (!string.IsNullOrWhiteSpace(request.DocumentName))
        {
            document.DocumentName = request.DocumentName;
            document.NormalizedName = request.DocumentName.ToLowerInvariant();
        }
        if (!string.IsNullOrWhiteSpace(request.Description))
            document.Description = request.Description;

        await ValidateSubjectAndTypeAsync(request.SubjectId, request.TypeId, ct);
        document.SubjectId = request.SubjectId;
        document.TypeId = request.TypeId;

        var tagIds = await ResolveTagIdsAsync(request.TagIds, request.TagNames, userId, ct);
        if (!tagIds.Any())
            throw new BadRequestException("Document must have at least one tag");

        var authorIds = await ResolveAuthorIdsAsync(request.AuthorIds, request.Authors, userId, ct);

        if (request.Visibility.HasValue)
            document.Visibility = request.Visibility.Value;

        AttachTags(document, tagIds);
        AttachAuthors(document, authorIds);

        if (document.CoverFileId.HasValue)
        {
            var file = await fileUsageService.EnsureFileAsync(document.CoverFileId.Value, ct);
            await fileUsageService.DeleteFileAsync(file, ct);
        }
        await AttachCoverFileAsync(document, request.CoverFileId, ct);

        document.UpdatedById = userId;
        document.UpdatedAt = dateTimeProvider.OffsetNow;

        documentRepository.Update(document);
        await documentRepository.UnitOfWork.SaveChangesAsync(ct);

        return await queryService.GetDetailByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Document with id {request.Id} not found");
    }

    // Helper methods
    private static void AttachTags(DocumentEntity document, IEnumerable<Guid> tagIds)
    {
        document.DocumentTags.Clear();
        foreach (var tagId in tagIds)
            document.DocumentTags.Add(new DocumentTag { DocumentId = document.Id, TagId = tagId });
    }

    private static void AttachAuthors(DocumentEntity document, IEnumerable<Guid> authorIds)
    {
        document.DocumentAuthors.Clear();
        foreach (var authorId in authorIds)
            document.DocumentAuthors.Add(new DocumentAuthor { DocumentId = document.Id, AuthorId = authorId });
    }

    private async Task<List<Guid>> ResolveTagIdsAsync(IList<Guid>? ids, IList<string>? names, Guid userId, CancellationToken ct)
    {
        var tagIds = new HashSet<Guid>();

        if (ids?.Any() == true)
        {
            var tags = await tagRepository.GetByIdsAsync(ids, ct);
            if (tags.Count != ids.Count)
                throw new NotFoundException("One or more tags not found");
            foreach (var tag in tags)
                tagIds.Add(tag.Id);
        }

        if (names?.Any() == true)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;

                var normalized = name.Trim();
                var existing = await tagRepository.FindByNameAsync(normalized, ct);
                if (existing != null)
                {
                    tagIds.Add(existing.Id);
                    continue;
                }

                var titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(normalized.ToLowerInvariant());
                var newTag = new TagEntity
                {
                    Id = Guid.NewGuid(),
                    TagName = titleCase,
                    Status = ContentStatus.Approved,
                    CreatedById = userId,
                    CreatedAt = dateTimeProvider.OffsetNow
                };
                tagRepository.Add(newTag);
                tagIds.Add(newTag.Id);
            }
        }
        return tagIds.ToList();
    }

    private async Task<List<Guid>> ResolveAuthorIdsAsync(IList<Guid>? ids, IList<AuthorInput>? inputs, Guid userId, CancellationToken ct)
    {
        var authorIds = new HashSet<Guid>();

        if (ids?.Any() == true)
        {
            var authors = await authorRepository.GetByIdsAsync(ids, ct);
            if (authors.Count != ids.Count)
                throw new NotFoundException("One or more authors not found");
            foreach (var author in authors)
                authorIds.Add(author.Id);
        }

        if (inputs?.Any() == true)
        {
            foreach (var input in inputs)
            {
                if (string.IsNullOrWhiteSpace(input.FullName)) continue;

                var normalized = input.FullName.Trim();
                var existing = await authorRepository.FindByNameAsync(normalized, ct);
                if (existing != null)
                {
                    authorIds.Add(existing.Id);
                    continue;
                }

                var newAuthor = new AuthorEntity
                {
                    Id = Guid.NewGuid(),
                    FullName = normalized,
                    Description = input.Description ?? string.Empty,
                    Status = ContentStatus.Approved,
                    CreatedById = userId,
                    CreatedAt = dateTimeProvider.OffsetNow
                };
                authorRepository.Add(newAuthor);
                authorIds.Add(newAuthor.Id);
            }
        }
        return authorIds.ToList();
    }

    private async Task ValidateSubjectAndTypeAsync(Guid? subjectId, Guid typeId, CancellationToken ct)
    {
        if (subjectId.HasValue)
        {
            var subject = await subjectRepository.GetByIdAsync(subjectId.Value, true, ct);
            if (subject == null)
                throw new NotFoundException($"Subject with id {subjectId} not found");
        }
        var type = await typeRepository.GetByIdAsync(typeId, true, ct);
        if (type == null)
            throw new NotFoundException($"Type with id {typeId} not found");
    }

    private async Task AttachCoverFileAsync(DocumentEntity document, Guid? coverFileId, CancellationToken ct)
    {
        if (!coverFileId.HasValue) return;
        if (coverFileId.Value == Guid.Empty)
            throw new BadRequestException("CoverFileId is invalid.");

        var coverFile = await fileUsageService.EnsureFileAsync(coverFileId.Value, ct);
        document.CoverFileId = coverFile.Id;
    }
}