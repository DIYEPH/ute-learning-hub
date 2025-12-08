using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Domain.Entities.Base;

public static class ReviewableExtensions
{
    public static void MarkAsReviewed(
        this IReviewable entity,
        ReviewStatus status,
        Guid reviewedById,
        DateTimeOffset? reviewedAt = null,
        string? reviewNote = null)
    {
        entity.ReviewStatus = status;
        entity.ReviewedById = reviewedById;
        entity.ReviewedAt = reviewedAt ?? DateTimeOffset.UtcNow;
        entity.ReviewNote = reviewNote;
    }

    public static void Approve(this IReviewable entity, Guid reviewedById, string? reviewNote = null)
    {
        entity.MarkAsReviewed(ReviewStatus.Approved, reviewedById, reviewNote: reviewNote);
    }

    public static void Reject(this IReviewable entity, Guid reviewedById, string? reviewNote = null)
    {
        entity.MarkAsReviewed(ReviewStatus.Rejected, reviewedById, reviewNote: reviewNote);
    }

    public static void Hide(this IReviewable entity, Guid reviewedById, string? reviewNote = null)
    {
        entity.MarkAsReviewed(ReviewStatus.Hidden, reviewedById, reviewNote: reviewNote);
    }

    public static void ResetReview(this IReviewable entity)
    {
        entity.ReviewStatus = ReviewStatus.PendingReview;
        entity.ReviewedById = null;
        entity.ReviewedAt = null;
        entity.ReviewNote = null;
    }
}
