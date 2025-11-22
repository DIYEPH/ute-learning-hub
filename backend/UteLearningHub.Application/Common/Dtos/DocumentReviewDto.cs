using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record DocumentReviewDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public DocumentReviewType DocumentReviewType { get; init; }
    public Guid CreatedById { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public record DocumentReviewStatsDto
{
    public Guid DocumentId { get; init; }
    public int UsefulCount { get; init; }
    public int NotUsefulCount { get; init; }
    public int TotalCount { get; init; }
    public DocumentReviewType? CurrentUserReview { get; init; }
}
