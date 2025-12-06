using MediatR;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.File;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocumentFile;

public class DeleteDocumentFileCommandHandler : IRequestHandler<DeleteDocumentFileCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileUsageService _fileUsageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteDocumentFileCommandHandler(
        IDocumentRepository documentRepository,
        IFileUsageService fileUsageService,
        ICurrentUserService currentUserService,
        IUserService userService,
        IDateTimeProvider dateTimeProvider)
    {
        _documentRepository = documentRepository;
        _fileUsageService = fileUsageService;
        _currentUserService = currentUserService;
        _userService = userService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(DeleteDocumentFileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to delete document files");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var document = await _documentRepository.GetByIdWithDetailsAsync(request.DocumentId, disableTracking: false, cancellationToken);

        if (document == null || document.IsDeleted)
            throw new NotFoundException($"Document with id {request.DocumentId} not found");

        var isOwner = document.CreatedById == userId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var trustLevel = await _userService.GetTrustLevelAsync(userId, cancellationToken);

        var canUpdate = isOwner ||
                        isAdmin ||
                        (trustLevel.HasValue && trustLevel.Value >= TrustLever.Moderator);

        if (!canUpdate)
            throw new UnauthorizedException("You don't have permission to delete this document file");

        var fileEntity = document.DocumentFiles.FirstOrDefault(df => df.Id == request.DocumentFileId && !df.IsDeleted);

        if (fileEntity == null)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found in this document");

        fileEntity.MarkAsDeleted(userId, _dateTimeProvider.OffsetNow);

        await _documentRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        var file = await _fileUsageService.EnsureFileAsync(fileEntity.FileId, cancellationToken);

        var otherUsage = await _documentRepository.GetQueryableSet()
            .Where(d => !d.IsDeleted)
            .SelectMany(d => d.DocumentFiles)
            .AnyAsync(df => df.FileId == fileEntity.FileId && !df.IsDeleted && df.Id != fileEntity.Id, cancellationToken);

        if (!otherUsage)
        {
            await _fileUsageService.DeleteFileAsync(file, cancellationToken);
        }
    }
}


