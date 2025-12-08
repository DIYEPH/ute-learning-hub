using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.DocumentFiles.Commands.ReviewDocumentFile;

public record ReviewDocumentFileRequest
{
    public Guid DocumentFileId { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
    public string? ReviewNote { get; init; }
}
