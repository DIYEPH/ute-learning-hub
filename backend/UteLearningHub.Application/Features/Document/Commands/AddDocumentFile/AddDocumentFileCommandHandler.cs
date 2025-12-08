using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.FileStorage;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.Settings;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public class AddDocumentFileCommandHandler : IRequestHandler<AddDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentPageCountService _documentPageCountService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IVectorMaintenanceService _vectorUpdateService;
    private readonly ISystemSettingService _systemSettingService;
    private readonly IDocumentQueryService _documentQueryService;

    public AddDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        IFileRepository fileRepository,
        IFileStorageService fileStorageService,
        IDocumentPageCountService documentPageCountService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IVectorMaintenanceService vectorUpdateService,
        ISystemSettingService systemSettingService,
        IDocumentQueryService documentQueryService)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _fileRepository = fileRepository;
        _fileStorageService = fileStorageService;
        _documentPageCountService = documentPageCountService;
        _currentUserService = currentUserService;
        _userService = userService;
        _vectorUpdateService = vectorUpdateService;
        _systemSettingService = systemSettingService;
        _documentQueryService = documentQueryService;
    }

    public async Task<DocumentDetailDto> Handle(AddDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to modify documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: true, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner ||
                        isAdmin ||
                        (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to add chapters to this document");

        if (request.FileId == Guid.Empty)
            throw new BadRequestException("FileId is required");

        await _fileUsageService.EnsureFileAsync(request.FileId, cancellationToken);

        Guid? coverFileId = null;
        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");

            await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);
            coverFileId = request.CoverFileId.Value;
        }

        var nextOrder = document.DocumentFiles.Any()
            ? document.DocumentFiles.Max(df => df.Order) + 1
            : 1;

        int? totalPages = request.TotalPages;
        if (!totalPages.HasValue)
        {
            var file = await _fileRepository.GetByIdAsync(request.FileId, disableTracking: true, cancellationToken);
            if (file != null)
            {
                try
                {
                    var fileStream = await _fileStorageService.GetFileAsync(file.FileUrl, cancellationToken);
                    if (fileStream != null)
                    {
                        totalPages = await _documentPageCountService.GetPageCountAsync(
                            fileStream,
                            file.MimeType ?? "",
                            cancellationToken);
                    }
                }
                catch
                {
                    totalPages = null;
                }
            }
        }

        var createDocumentSetting = await _systemSettingService.GetIntAsync(SystemName.CreateDocument, 0, cancellationToken);
        
        ReviewStatus reviewStatus;
        if (isAdmin)
            reviewStatus = ReviewStatus.Approved;
        else if (createDocumentSetting == 0)
            reviewStatus = ReviewStatus.Approved;
        else if (createDocumentSetting == 1)
            reviewStatus = (trustLevel.HasValue && trustLevel.Value >= TrustLever.TrustedMember)
                ? ReviewStatus.Approved
                : ReviewStatus.PendingReview;
        else
            reviewStatus = ReviewStatus.PendingReview;

        var fileOrder = request.Order ?? nextOrder;
        var defaultTitle = string.IsNullOrWhiteSpace(request.Title) 
            ? $"{document.DocumentName} ({fileOrder})" 
            : request.Title;
            
        var chapter = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            FileId = request.FileId,
            Title = defaultTitle,
            TotalPages = totalPages,
            IsPrimary = request.IsPrimary,
            Order = fileOrder,
            CreatedById = userId,
            UpdatedById = null,
            CoverFileId = coverFileId,
            ReviewStatus = reviewStatus
        };

        await _documentRepository.AddDocumentFileAsync(chapter, cancellationToken);
        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _ = Task.Run(() => _vectorUpdateService.UpdateUserVectorAsync(userId, cancellationToken));

        var result = await _documentQueryService.GetDetailByIdAsync(request.DocumentId, cancellationToken);
        return result ?? throw new NotFoundException($"Document with id {request.DocumentId} not found after update");
    }
}