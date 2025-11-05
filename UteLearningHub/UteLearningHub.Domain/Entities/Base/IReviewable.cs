using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Domain.Entities.Base;

public interface IReviewable
{
    Guid? ReviewedById { get; set; }
    string? ReviewNote { get; set; }
    DateTimeOffset? ReviewedAt { get; set; }
    ReviewStatus ReviewStatus { get; set; }
}
