using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Settings;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Infrastructure.Services.Document;

public class DocumentFileService(
    IDocumentRepository documentRepository,
    ICurrentUserService currrentUserService,
    IFileUsageService fileUsageService,
    IFileRepository fileRepository,
    IFileStorageService fileStorageService,
    IDocumentPageCountService documentPageCountService,
    ISystemSettingService systemSettingService,
    IUserService userService,
    ITrustScoreService trustScoreService,
    IDocumentQueryService documentQueryService,
    IDocumentReviewRepository documentReviewRepository,
    IDateTimeProvider dateTimeProvider,
    IMediator mediator
    ) : IDocumentFileService
{
    private readonly IDocumentRepository _documentRepository = documentRepository;
    private readonly ICurrentUserService _currentUserService = currrentUserService;
    private readonly IFileUsageService _fileUsageService = fileUsageService;
    private readonly IFileRepository _fileRepository = fileRepository;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IDocumentPageCountService _documentPageCountService = documentPageCountService;
    private readonly ISystemSettingService _systemSettingService = systemSettingService;
    private readonly IUserService _userService = userService;
    private readonly ITrustScoreService _trustScoreService = trustScoreService;
    private readonly IDocumentQueryService _documentQueryService = documentQueryService;
    private readonly IDocumentReviewRepository _documentReviewRepository = documentReviewRepository;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IMediator _mediator = mediator;
    public async Task<DocumentDetailDto> CreateAsync(AddDocumentFileCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();


        if (request.FileId == Guid.Empty || request.DocumentId == Guid.Empty)
            throw new BadRequestException("FileId Or DocumentId is required");

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, true, ct);

        if (document == null)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var canAdd = isOwner || isAdmin;

        if (!canAdd)
            throw new UnauthorizedException("You don't have permission to add chapters to this document");

        await _fileUsageService.EnsureFileAsync(request.FileId, ct);

        Guid? coverFileId = null;
        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");

            await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, ct);
            coverFileId = request.CoverFileId.Value;
        }

        var nextOrder = document.DocumentFiles.Any() ? document.DocumentFiles.Max(df => df.Order) + 1 : 1;
        int? totalPages = null;

        var file = await _fileRepository.GetByIdAsync(request.FileId, true, ct);
        if (file != null)
        {
            try
            {
                var fileStream = await _fileStorageService.GetFileAsync(file.FileUrl, ct);
                if (fileStream != null)
                    totalPages = await _documentPageCountService.GetPageCountAsync(fileStream, file.MimeType, ct);
            }
            catch
            {
                totalPages = 1;
            }
        }

        var createDocumentSetting = await _systemSettingService.GetIntAsync(SystemName.CreateDocument, 0, ct);

        ContentStatus status;
        var trustLevel = await _userService.GetTrustLevelAsync(userId, ct);

        if (isAdmin || createDocumentSetting == 0)
            status = ContentStatus.Approved;
        else if (createDocumentSetting == 1 && trustLevel >= TrustLever.TrustedMember)
            status = ContentStatus.Approved;
        else
            status = ContentStatus.PendingReview;

        var fileOrder = nextOrder;
        var defaultTitle = string.IsNullOrWhiteSpace(request.Title) ? $"({fileOrder}). {document.DocumentName}" : request.Title;

        var chapter = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            FileId = request.FileId,
            Title = defaultTitle,
            TotalPages = totalPages,
            Order = fileOrder,
            CreatedById = userId,
            UpdatedById = null,
            CoverFileId = coverFileId,
            Status = status
        };

        _documentRepository.AddDocumentFile(chapter);
        await _documentRepository.UnitOfWork.SaveChangesAsync(ct);

        await _trustScoreService.AddTrustScoreAsync(userId, TrustScoreConstants.GetActionPoints("CreateDocumentFile"), "Thêm chương/file tài liệu", chapter.Id, TrustEntityType.DocumentFile, ct);

        var result = await _documentQueryService.GetDetailByIdAsync(request.DocumentId, ct);
        try
        {
            await _mediator.Publish(new UserActivityEvent
            {
                UserId = userId,
                ActivityType = "DocumentFileUploaded",
                Metadata = new Dictionary<string, object>
                {
                    { "DocumentId", request.DocumentId },
                    { "FileId", request.FileId }
                }
            }, ct);
        }
        catch
        {
        }

        return result ?? throw new NotFoundException($"Document with id {request.DocumentId} not found after update");
    }

    public async Task CreateOrUpdateDocumentFile(CreateOrUpdateDocumentFileReviewCommand request, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(request.DocumentFileId, true, ct);
        if (documentFile == null)
            throw new NotFoundException($"Documentfile with id {request.DocumentFileId} not found");

        var document = await _documentRepository.GetByIdAsync(documentFile.DocumentId, true, ct);
        var creatorId = document?.CreatedById;

        var exists = await _documentReviewRepository.GetByIdAndUserIdAsync(request.DocumentFileId, userId, cancellationToken: ct);

        DocumentReviewType? oldType = exists?.DocumentReviewType;
        DocumentReview review;
        //1. same -> delte
        if (exists != null && exists.DocumentReviewType == request.DocumentReviewType)
        {
            _documentReviewRepository.Delete(exists);
            await _documentReviewRepository.UnitOfWork.SaveChangesAsync(ct);

            await _mediator.Publish(new DocumentReviewedEvent(request.DocumentFileId, userId, creatorId, oldType, null), ct);
            return;
        }
        // CASE 2 & 3: Update or Create
        if (exists != null)
        {
            exists.DocumentReviewType = request.DocumentReviewType;
            exists.UpdatedById = userId;
            exists.UpdatedAt = _dateTimeProvider.OffsetNow;
            _documentReviewRepository.Update(exists);
            review = exists;
        }
        else
        {
            review = new DocumentReview
            {
                DocumentId = documentFile.DocumentId,
                DocumentFileId = request.DocumentFileId,
                DocumentReviewType = request.DocumentReviewType,
                CreatedById = userId,
                CreatedAt = _dateTimeProvider.OffsetNow
            };
            _documentReviewRepository.Add(review);
        }

        await _documentReviewRepository.UnitOfWork.SaveChangesAsync(ct);

        await _mediator.Publish(new DocumentReviewedEvent(request.DocumentFileId, userId, creatorId, oldType, request.DocumentReviewType), ct);
    }


    public async Task IncrementViewCountAsync(Guid documentFileId, CancellationToken ct)
    {
        var docFile = await _documentRepository.GetDocumentFileByIdAsync(documentFileId, cancellationToken: ct);
        if (docFile == null)
            throw new NotFoundException($"DocumentFile with id {documentFileId} not found");

        docFile.ViewCount++;

        await _documentRepository.UnitOfWork.SaveChangesAsync(ct);

    }

    public async Task SoftDeleteAsync(Guid documentFileId, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var documentFile = await _documentRepository.GetDocumentFileByIdAsync(documentFileId, cancellationToken: ct);

        if (documentFile == null)
            throw new NotFoundException($"DocumentFile with id {documentFileId} not found");

        var document = await _documentRepository.GetByIdWithDetailsAsync(documentFile.DocumentId, cancellationToken: ct);

        var isOwner = document!.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");

        var canDelete = isOwner || isAdmin;

        if (!canDelete)
            throw new UnauthorizedException("You don't have permission to delete this document file");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == documentFileId);

        if (fileEntity == null)
            throw new NotFoundException($"Document file with id {documentFileId} not found in this document");

        await _trustScoreService.RevertTrustScoreByEntityAsync(fileEntity.Id, TrustEntityType.DocumentFile, ct);

        fileEntity.IsDeleted = true;
        fileEntity.DeletedById = userId;
        fileEntity.DeletedAt = _dateTimeProvider.OffsetUtcNow;

        _documentRepository.UpdateDocumentFile(fileEntity);
        await _documentRepository.UnitOfWork.SaveChangesAsync(ct);

        var remainingFiles = document.DocumentFiles.Count(df => df.Id != documentFileId);
        if (remainingFiles == 0)
        {
            document.IsDeleted = true;
            document.DeletedById = userId;
            document.DeletedAt = _dateTimeProvider.OffsetUtcNow;
            await _documentRepository.UnitOfWork.SaveChangesAsync(ct);
        }

        // Soft delete associated file (mark as deleted, not hard delete)
        var file = await _fileRepository.GetByIdAsync(fileEntity.FileId, disableTracking: false, ct);
        if (file != null)
        {
            file.IsDeleted = true;
            file.DeletedById = userId;
            file.DeletedAt = _dateTimeProvider.OffsetUtcNow;
            _fileRepository.Update(file);
            await _fileRepository.UnitOfWork.SaveChangesAsync(ct);
        }
    }

    public Task<DocumentDetailDto> UpdateAsync(UpdateDocumentCommand request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
