using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto>
{
    private readonly IDocumentQueryService _documentQueryService;
    private readonly IUserDocumentProgressRepository _progressRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDocumentByIdHandler(
        IDocumentQueryService documentQueryService,
        IUserDocumentProgressRepository progressRepository,
        ICurrentUserService currentUserService)
    {
        _documentQueryService = documentQueryService;
        _progressRepository = progressRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DocumentDetailDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUserService.IsAuthenticated && _currentUserService.IsInRole("Admin");
        var isAuthenticated = _currentUserService.IsAuthenticated;
        var userId = _currentUserService.UserId;

        var document = await _documentQueryService.GetDetailByIdAsync(request.Id, cancellationToken);
        
        if (document == null)
            throw new NotFoundException($"Document with id {request.Id} not found");

        var isOwner = isAuthenticated && userId.HasValue && document.CreatedById == userId.Value;

        // Owner và Admin có thể xem document với mọi trạng thái file
        // Non-owner/non-admin cần ít nhất 1 file đã được approved
        var hasApprovedFile = document.Files.Any(f => f.Status == ContentStatus.Approved);
        if (!isAdmin && !isOwner && !hasApprovedFile)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Người chưa đăng nhập chỉ xem được tài liệu Public
        if (!isAuthenticated && document.Visibility != VisibilityStatus.Public)
            throw new NotFoundException($"Document with id {request.Id} not found");

        // Load progress cho user hiện tại (nếu đã authenticated)
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            var progressList = await _progressRepository.GetByUserAndDocumentAsync(
                _currentUserService.UserId.Value,
                request.Id,
                disableTracking: true,
                cancellationToken);

            var progressDict = progressList.ToDictionary(
                p => p.DocumentFileId ?? Guid.Empty,
                p => new DocumentProgressDto
                {
                    DocumentFileId = p.DocumentFileId ?? Guid.Empty,
                    LastPage = p.LastPage,
                    TotalPages = p.TotalPages,
                    LastAccessedAt = p.LastAccessedAt
                });

            // Update files with progress
            return document with
            {
                Files = document.Files.Select(f =>
                {
                    progressDict.TryGetValue(f.Id, out var progress);
                    return f with { Progress = progress };
                }).ToList()
            };
        }

        return document;
    }
}
