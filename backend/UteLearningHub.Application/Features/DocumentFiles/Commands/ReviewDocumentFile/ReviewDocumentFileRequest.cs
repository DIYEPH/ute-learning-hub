using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.DocumentFiles.Commands.ReviewDocumentFile;

public record ReviewDocumentFileRequest
{
    public Guid DocumentFileId { get; init; }
    public ContentStatus Status { get; init; }
}

