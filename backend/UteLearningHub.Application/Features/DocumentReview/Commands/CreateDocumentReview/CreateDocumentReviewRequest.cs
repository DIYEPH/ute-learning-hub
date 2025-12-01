using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.DocumentReview.Commands.CreateDocumentReview;

public record CreateDocumentReviewRequest
{
    public Guid DocumentFileId { get; init; }
    public DocumentReviewType DocumentReviewType { get; init; }
}
