using MediatR;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.EventHandlers;

public class DocumentReviewedEventHandler(ITrustScoreService trustScoreService, IVectorMaintenanceService vectorService) : INotificationHandler<DocumentReviewedEvent>
{
    private readonly ITrustScoreService _trustScoreService = trustScoreService;
    private readonly IVectorMaintenanceService _vectorService = vectorService;

    public async Task Handle(DocumentReviewedEvent notification, CancellationToken ct)
    {
        try
        {
            await HandleTrustScoreAsync(notification, ct);
            await HandleVectorAsync(notification, ct);
        }
        catch
        {
        }
    }

    private async Task HandleTrustScoreAsync(
        DocumentReviewedEvent e,
        CancellationToken ct)
    {
        if (!e.CreatorId.HasValue)
            return;

        if (e.CreatorId.Value == e.ReviewerId)
            return;

        // 1️⃣ Hoàn điểm cũ (nếu có)
        if (e.OldType == DocumentReviewType.Useful)
        {
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, -TrustScoreConstants.GetActionPoints("DocumentLiked"), "Unlike document file", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        }
        else if (e.OldType == DocumentReviewType.NotUseful)
        {
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, -TrustScoreConstants.GetActionPoints("DocumentUnliked"), "Undo dislike document", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        }

        // 2️⃣ Áp dụng điểm mới (nếu có)
        if (e.NewType == DocumentReviewType.Useful)
        {
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, TrustScoreConstants.GetActionPoints("DocumentLiked"), "Document liked", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        }
        else if (e.NewType == DocumentReviewType.NotUseful)
        {
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, TrustScoreConstants.GetActionPoints("DocumentUnliked"), "Document disliked", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        }
    }
    private async Task HandleVectorAsync(
        DocumentReviewedEvent e,
        CancellationToken ct)
    {
        await _vectorService.UpdateUserVectorAsync(e.ReviewerId, ct);
    }
}