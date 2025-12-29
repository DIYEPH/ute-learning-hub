using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;

public record CreateOrUpdateDocumentFileReviewCommand : IRequest
{
    public Guid DocumentFileId { get; init; }
    public DocumentReviewType DocumentReviewType { get; init; }
}
