using MediatR;
using UteLearningHub.Application.Events;
using UteLearningHub.Application.Services.Recommendation;
using UteLearningHub.Application.Services.TrustScore;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Policies;

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
            _ = Task.Run(() => HandleVectorAsync(notification, ct));
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

        // Hoàn điểm cũ
        if (e.OldType == DocumentReviewType.Useful)
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, -TrustScorePolicy.GetActionPoints("DocumentLiked"), "Unlike document file", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        else if (e.OldType == DocumentReviewType.NotUseful)
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, -TrustScorePolicy.GetActionPoints("DocumentUnliked"), "Undo dislike document", e.DocumentFileId, TrustEntityType.DocumentFile, ct);

        // Áp dụng điểm mới
        if (e.NewType == DocumentReviewType.Useful)
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, TrustScorePolicy.GetActionPoints("DocumentLiked"), "Document liked", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
        else if (e.NewType == DocumentReviewType.NotUseful)
            await _trustScoreService.AddTrustScoreAsync(e.CreatorId.Value, TrustScorePolicy.GetActionPoints("DocumentUnliked"), "Document disliked", e.DocumentFileId, TrustEntityType.DocumentFile, ct);
    }
    private async Task HandleVectorAsync(
        DocumentReviewedEvent e,
        CancellationToken ct)
    {
        await _vectorService.UpdateUserVectorAsync(e.ReviewerId, ct);
    }
}