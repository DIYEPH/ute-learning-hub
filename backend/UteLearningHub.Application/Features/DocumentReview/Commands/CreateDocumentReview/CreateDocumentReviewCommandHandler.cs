using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.CrossCuttingConcerns.DateTimes;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;
using DocumentReviewEntity = UteLearningHub.Domain.Entities.DocumentReview;

namespace UteLearningHub.Application.Features.DocumentReview.Commands.CreateDocumentReview;

public class CreateDocumentReviewCommandHandler : IRequestHandler<CreateDocumentReviewCommand, DocumentReviewDto>
{
    private readonly IDocumentReviewRepository _documentReviewRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IVectorMaintenanceService? _vectorMaintenanceService;
    private readonly ITrustScoreService? _trustScoreService;

    public CreateDocumentReviewCommandHandler(
        IDocumentReviewRepository documentReviewRepository,
        IDocumentRepository documentRepository,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider,
        IVectorMaintenanceService? vectorMaintenanceService = null,
        ITrustScoreService? trustScoreService = null)
    {
        _documentReviewRepository = documentReviewRepository;
        _documentRepository = documentRepository;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _vectorMaintenanceService = vectorMaintenanceService;
        _trustScoreService = trustScoreService;
    }

    public async Task<DocumentReviewDto> Handle(CreateDocumentReviewCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to review documents");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var documentId = await _documentRepository.GetDocumentIdByDocumentFileIdAsync(request.DocumentFileId, cancellationToken);
        if (!documentId.HasValue)
            throw new NotFoundException($"Document file with id {request.DocumentFileId} not found");

        // Lấy thông tin document để biết người tạo
        var document = await _documentRepository.GetByIdAsync(documentId.Value, disableTracking: true, cancellationToken);
        var documentCreatorId = document?.CreatedById;

        var existingReview = await _documentReviewRepository.GetByDocumentFileIdAndUserIdAsync(
            request.DocumentFileId,
            userId,
            disableTracking: false,
            cancellationToken);
        if (existingReview != null && existingReview.DocumentReviewType == request.DocumentReviewType)
        {
            // Xóa review (unlike/un-dislike) - trừ điểm cho người tạo document
            await _documentReviewRepository.DeleteAsync(existingReview, userId, cancellationToken);
            await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Trừ điểm cho người tạo document khi bị unlike (async)
            if (_trustScoreService != null && documentCreatorId.HasValue && 
                existingReview.DocumentReviewType == DocumentReviewType.Useful)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var points = -TrustScoreConstants.GetActionPoints("DocumentLiked");
                        await _trustScoreService.AddTrustScoreAsync(
                            documentCreatorId.Value,
                            points,
                            $"Bị unlike document",
                            cancellationToken);
                    }
                    catch
                    {
                        // Log error nhưng không throw
                    }
                }, cancellationToken);
            }

            return new DocumentReviewDto
            {
                Id = existingReview.Id,
                DocumentId = existingReview.DocumentId,
                DocumentFileId = existingReview.DocumentFileId,
                DocumentReviewType = existingReview.DocumentReviewType,
                CreatedById = existingReview.CreatedById,
                CreatedAt = existingReview.CreatedAt,
                UpdatedAt = _dateTimeProvider.OffsetNow
            };
        }

        DocumentReviewEntity review;

        if (existingReview != null)
        {
            existingReview.DocumentReviewType = request.DocumentReviewType;
            existingReview.UpdatedById = userId;
            existingReview.UpdatedAt = _dateTimeProvider.OffsetNow;
            await _documentReviewRepository.UpdateAsync(existingReview, cancellationToken);
            review = existingReview;
        }
        else
        {
            review = new DocumentReviewEntity
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId.Value,
                DocumentFileId = request.DocumentFileId,
                DocumentReviewType = request.DocumentReviewType,
                CreatedById = userId,
                CreatedAt = _dateTimeProvider.OffsetNow
            };
            await _documentReviewRepository.AddAsync(review, cancellationToken);
        }

        await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Cập nhật trust score cho người tạo document khi được like/unlike (async)
        if (_trustScoreService != null && documentCreatorId.HasValue && documentCreatorId.Value != userId)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    int points;
                    string reason;
                    
                    if (review.DocumentReviewType == DocumentReviewType.Useful)
                    {
                        // Được like - cộng điểm
                        points = TrustScoreConstants.GetActionPoints("DocumentLiked");
                        reason = "Document được like";
                    }
                    else if (review.DocumentReviewType == DocumentReviewType.NotUseful)
                    {
                        // Bị unlike - trừ điểm
                        points = TrustScoreConstants.GetActionPoints("DocumentUnliked");
                        reason = "Document bị unlike";
                    }
                    else
                    {
                        return; // Không xử lý các loại review khác
                    }

                    // Nếu có review cũ, cần hoàn lại điểm
                    if (existingReview != null)
                    {
                        if (existingReview.DocumentReviewType == DocumentReviewType.Useful)
                        {
                            // Hoàn lại điểm like cũ
                            await _trustScoreService.AddTrustScoreAsync(
                                documentCreatorId.Value,
                                -TrustScoreConstants.GetActionPoints("DocumentLiked"),
                                "Hoàn lại điểm like cũ",
                                cancellationToken);
                        }
                        else if (existingReview.DocumentReviewType == DocumentReviewType.NotUseful)
                        {
                            // Hoàn lại điểm unlike cũ
                            await _trustScoreService.AddTrustScoreAsync(
                                documentCreatorId.Value,
                                -TrustScoreConstants.GetActionPoints("DocumentUnliked"),
                                "Hoàn lại điểm unlike cũ",
                                cancellationToken);
                        }
                    }

                    await _trustScoreService.AddTrustScoreAsync(
                        documentCreatorId.Value,
                        points,
                        reason,
                        cancellationToken);
                }
                catch
                {
                    // Log error nhưng không throw
                }
            }, cancellationToken);
        }

        // Cập nhật user vector khi user like/unlike document (async, không block response)
        if (_vectorMaintenanceService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _vectorMaintenanceService.UpdateUserVectorAsync(userId, cancellationToken);
                }
                catch
                {
                    // Log error nhưng không throw để không ảnh hưởng đến response
                    // Logger có thể được inject nếu cần
                }
            }, cancellationToken);
        }

        return new DocumentReviewDto
        {
            Id = review.Id,
            DocumentId = review.DocumentId,
            DocumentFileId = review.DocumentFileId,
            DocumentReviewType = review.DocumentReviewType,
            CreatedById = review.CreatedById,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }
}
