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
        var canUpdateTrustScore = _trustScoreService != null && documentCreatorId.HasValue && documentCreatorId.Value != userId;

        var existingReview = await _documentReviewRepository.GetByDocumentFileIdAndUserIdAsync(
            request.DocumentFileId,
            userId,
            disableTracking: false,
            cancellationToken);

        // CASE 1: Same type clicked again → Toggle off (Delete)
        if (existingReview != null && existingReview.DocumentReviewType == request.DocumentReviewType)
        {
            var oldType = existingReview.DocumentReviewType;
            await _documentReviewRepository.DeleteAsync(existingReview, userId, cancellationToken);
            await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            // Hoàn lại điểm đã cộng/trừ trước đó
            if (canUpdateTrustScore)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (oldType == DocumentReviewType.Useful)
                        {
                            // Was liked → undo like = -points
                            var points = -TrustScoreConstants.GetActionPoints("DocumentLiked");
                            await _trustScoreService!.AddTrustScoreAsync(documentCreatorId!.Value, points, "Hủy like document", request.DocumentFileId, cancellationToken);
                        }
                        else if (oldType == DocumentReviewType.NotUseful)
                        {
                            // Was disliked → undo dislike = +points (hoàn lại điểm bị trừ)
                            var points = -TrustScoreConstants.GetActionPoints("DocumentUnliked");
                            await _trustScoreService!.AddTrustScoreAsync(documentCreatorId!.Value, points, "Hủy dislike document", request.DocumentFileId, cancellationToken);
                        }
                    }
                    catch { }
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
        DocumentReviewType? oldReviewType = existingReview?.DocumentReviewType;

        // CASE 2: Different type clicked → Change vote (Update)
        // CASE 3: No existing review → Create new
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
                DocumentId = documentId.Value,
                DocumentFileId = request.DocumentFileId,
                DocumentReviewType = request.DocumentReviewType,
                CreatedById = userId,
                CreatedAt = _dateTimeProvider.OffsetNow
            };
            await _documentReviewRepository.AddAsync(review, cancellationToken);
        }

        await _documentReviewRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        // Cập nhật trust score
        if (canUpdateTrustScore)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    // Step 1: Hoàn lại điểm cũ (nếu có)
                    if (oldReviewType.HasValue)
                    {
                        if (oldReviewType.Value == DocumentReviewType.Useful)
                        {
                            // Was liked → undo like = -points
                            await _trustScoreService!.AddTrustScoreAsync(
                                documentCreatorId!.Value,
                                -TrustScoreConstants.GetActionPoints("DocumentLiked"),
                                "Đổi từ like sang dislike - hoàn điểm like",
                                request.DocumentFileId,
                                cancellationToken);
                        }
                        else if (oldReviewType.Value == DocumentReviewType.NotUseful)
                        {
                            // Was disliked → undo dislike = +points
                            await _trustScoreService!.AddTrustScoreAsync(
                                documentCreatorId!.Value,
                                -TrustScoreConstants.GetActionPoints("DocumentUnliked"),
                                "Đổi từ dislike sang like - hoàn điểm dislike",
                                request.DocumentFileId,
                                cancellationToken);
                        }
                    }

                    // Step 2: Áp dụng điểm mới
                    if (request.DocumentReviewType == DocumentReviewType.Useful)
                    {
                        await _trustScoreService!.AddTrustScoreAsync(
                            documentCreatorId!.Value,
                            TrustScoreConstants.GetActionPoints("DocumentLiked"),
                            "Document được like",
                            request.DocumentFileId,
                            cancellationToken);
                    }
                    else if (request.DocumentReviewType == DocumentReviewType.NotUseful)
                    {
                        await _trustScoreService!.AddTrustScoreAsync(
                            documentCreatorId!.Value,
                            TrustScoreConstants.GetActionPoints("DocumentUnliked"),
                            "Document bị dislike",
                            request.DocumentFileId,
                            cancellationToken);
                    }
                }
                catch { }
            }, cancellationToken);
        }

        // Update vector
        if (_vectorMaintenanceService != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _vectorMaintenanceService.UpdateUserVectorAsync(userId, cancellationToken);
                }
                catch { }
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

