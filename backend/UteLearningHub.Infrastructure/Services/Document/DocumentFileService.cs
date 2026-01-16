using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Settings;
using UteLearningHub.Domain.Policies;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using UteLearningHub.Application.Services.TrustScore;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentFileService(
    IDocumentRepository documentRepository,
    ICurrentUserService currentUserService,
    IFileUsageService fileUsageService,
    IFileRepository fileRepository,
    ISystemSettingService systemSettingService,
    IUserService userService,
    ITrustScoreService trustScoreService,
    IDocumentQueryService documentQueryService,
    IDocumentReviewRepository documentReviewRepository,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator) : IDocumentFileService
{
    public async Task<DocumentDetailDto> CreateAsync(AddDocumentFileCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        if (request.FileId == Guid.Empty || request.DocumentId == Guid.Empty)
            throw new BadRequestException("FileId Or DocumentId is required");

        var document = await documentRepository.GetByIdWithDetailsAsync(request.DocumentId, true, ct)
            ?? throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isOwner && !isAdmin)
            throw new UnauthorizedException("You don't have permission to add chapters to this document");

        await fileUsageService.EnsureFileAsync(request.FileId, ct);

        Guid? coverFileId = null;
        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");
            await fileUsageService.EnsureFileAsync(request.CoverFileId.Value, ct);
            coverFileId = request.CoverFileId.Value;
        }

        var nextOrder = document.DocumentFiles.Any() ? document.DocumentFiles.Max(df => df.Order) + 1 : 1;
        var createDocumentSetting = await systemSettingService.GetIntAsync(SystemName.CreateDocument, 0, ct);
        var trustLevel = await userService.GetTrustLevelAsync(userId, ct);

        var status = isAdmin || createDocumentSetting == 0
            ? ContentStatus.Approved
            : createDocumentSetting == 1 && trustLevel >= TrustLever.TrustedMember
                ? ContentStatus.Approved
                : ContentStatus.PendingReview;

        var defaultTitle = string.IsNullOrWhiteSpace(request.Title) ? $"({nextOrder}). {document.DocumentName}" : request.Title;

        var chapter = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            FileId = request.FileId,
            Title = defaultTitle,
            TotalPages = request.TotalPages,
            Order = nextOrder,
            CreatedById = userId,
            UpdatedById = null,
            CoverFileId = coverFileId,
            Status = status
        };

        documentRepository.AddDocumentFile(chapter);
        await documentRepository.UnitOfWork.SaveChangesAsync(ct);

        await trustScoreService.AddTrustScoreAsync(userId, TrustScorePolicy.GetActionPoints("CreateDocumentFile"), "Thêm chương/file tài liệu", chapter.Id, TrustEntityType.DocumentFile, ct);

        var result = await documentQueryService.GetDetailByIdAsync(request.DocumentId, ct);

        if (nextOrder == 1 && document.SubjectId.HasValue)
            _ = mediator.Publish(new DocumentFileCreatedEvent(request.DocumentId, chapter.Id, document.SubjectId, userId), ct);

        return result ?? throw new NotFoundException($"Document with id {request.DocumentId} not found after update");
    }

    public async Task CreateOrUpdateDocumentFile(CreateOrUpdateDocumentFileReviewCommand request, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();

        var documentFile = await documentRepository.GetDocumentFileByIdAsync(request.DocumentFileId, true, ct)
            ?? throw new NotFoundException($"DocumentFile with id {request.DocumentFileId} not found");

        var document = await documentRepository.GetByIdAsync(documentFile.DocumentId, true, ct);
        var creatorId = document?.CreatedById;

        var exists = await documentReviewRepository.GetByIdAndUserIdAsync(request.DocumentFileId, userId, cancellationToken: ct);
        DocumentReviewType? oldType = exists?.DocumentReviewType;

        if (exists != null && exists.DocumentReviewType == request.DocumentReviewType)
        {
            documentReviewRepository.Delete(exists);
            await documentReviewRepository.UnitOfWork.SaveChangesAsync(ct);
            await mediator.Publish(new DocumentReviewedEvent(request.DocumentFileId, userId, creatorId, oldType, null), ct);
            return;
        }

        if (exists != null)
        {
            exists.DocumentReviewType = request.DocumentReviewType;
            exists.UpdatedById = userId;
            exists.UpdatedAt = dateTimeProvider.OffsetNow;
            documentReviewRepository.Update(exists);
        }
        else
        {
            var review = new DocumentReview
            {
                DocumentId = documentFile.DocumentId,
                DocumentFileId = request.DocumentFileId,
                DocumentReviewType = request.DocumentReviewType,
                CreatedById = userId,
                CreatedAt = dateTimeProvider.OffsetNow
            };
            documentReviewRepository.Add(review);
        }

        await documentReviewRepository.UnitOfWork.SaveChangesAsync(ct);
        await mediator.Publish(new DocumentReviewedEvent(request.DocumentFileId, userId, creatorId, oldType, request.DocumentReviewType), ct);
    }

    public async Task IncrementViewCountAsync(Guid documentFileId, CancellationToken ct)
    {
        var docFile = await documentRepository.GetDocumentFileByIdAsync(documentFileId, cancellationToken: ct)
            ?? throw new NotFoundException($"DocumentFile with id {documentFileId} not found");
        docFile.ViewCount++;
        await documentRepository.UnitOfWork.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid documentFileId, CancellationToken ct)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedException();
        var documentFile = await documentRepository.GetDocumentFileByIdAsync(documentFileId, cancellationToken: ct)
            ?? throw new NotFoundException($"DocumentFile with id {documentFileId} not found");

        var document = await documentRepository.GetByIdWithDetailsAsync(documentFile.DocumentId, cancellationToken: ct)
            ?? throw new NotFoundException($"Document not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = currentUserService.IsInRole("Admin");
        if (!isOwner && !isAdmin)
            throw new UnauthorizedException("You don't have permission to delete this document file");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == documentFileId)
            ?? throw new NotFoundException($"Document file with id {documentFileId} not found in this document");

        await trustScoreService.RevertTrustScoreByEntityAsync(fileEntity.Id, TrustEntityType.DocumentFile, ct);

        fileEntity.IsDeleted = true;
        fileEntity.DeletedById = userId;
        fileEntity.DeletedAt = dateTimeProvider.OffsetUtcNow;
        documentRepository.UpdateDocumentFile(fileEntity);
        await documentRepository.UnitOfWork.SaveChangesAsync(ct);

        var remainingFiles = document.DocumentFiles.Count(df => df.Id != documentFileId);
        if (remainingFiles == 0)
        {
            document.IsDeleted = true;
            document.DeletedById = userId;
            document.DeletedAt = dateTimeProvider.OffsetUtcNow;
            await documentRepository.UnitOfWork.SaveChangesAsync(ct);
        }

        var file = await fileRepository.GetByIdAsync(fileEntity.FileId, disableTracking: false, ct);
        if (file != null)
        {
            file.IsDeleted = true;
            file.DeletedById = userId;
            file.DeletedAt = dateTimeProvider.OffsetUtcNow;
            fileRepository.Update(file);
            await fileRepository.UnitOfWork.SaveChangesAsync(ct);
        }
    }

    public Task<DocumentDetailDto> UpdateAsync(UpdateDocumentCommand request, CancellationToken ct)
        => throw new NotImplementedException();
}
