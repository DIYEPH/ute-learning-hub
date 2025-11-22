using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.ReviewDocument;

public record ReviewDocumentRequest
{
    public Guid DocumentId { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public string? ReviewNote { get; init; }
}