using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;

public class UpdateDocumentFileCommandHandler : IRequestHandler<UpdateDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDocumentQueryService _documentQueryService;

    public UpdateDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider,
        IDocumentQueryService documentQueryService)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
        _documentQueryService = documentQueryService;
    }

    public async Task<DocumentDetailDto> Handle(UpdateDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update document files");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: false, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner ||
                        isAdmin ||
                        trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator;

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to update this document file");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == request.DocumentFileId && !df.IsDeleted);

        if (fileEntity == null)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found in this document");

        if (!string.IsNullOrWhiteSpace(request.Title))
            fileEntity.Title = request.Title;

        if (request.Order.HasValue)
            fileEntity.Order = request.Order.Value;

        Domain.Entities.File? oldCoverFile = null;

        if (request.CoverFileId.HasValue)
        {
            if (request.CoverFileId.Value == Guid.Empty)
                throw new BadRequestException("CoverFileId is invalid");

            if (!fileEntity.CoverFileId.HasValue || fileEntity.CoverFileId.Value != request.CoverFileId.Value)
            {
                var newCoverFile = await _fileUsageService.EnsureFileAsync(request.CoverFileId.Value, cancellationToken);

                if (fileEntity.CoverFileId.HasValue)
                {
                    oldCoverFile = fileEntity.CoverFile ?? await _fileUsageService.EnsureFileAsync(fileEntity.CoverFileId.Value, cancellationToken);
                }

                fileEntity.CoverFileId = newCoverFile.Id;
            }
        }

        fileEntity.UpdatedById = userId;
        fileEntity.UpdatedAt = _dateTimeProvider.OffsetNow;

        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        if (oldCoverFile != null)
            await _fileUsageService.DeleteFileAsync(oldCoverFile, cancellationToken);

        var result = await _documentQueryService.GetDetailByIdAsync(request.DocumentId, cancellationToken);
        return result ?? throw new NotFoundException($"Document with id {request.DocumentId} not found after update");
    }
}
