using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.ResubmitDocumentFile;

public record ResubmitDocumentFileCommand : IRequest
{
    public Guid DocumentId { get; init; }
    public Guid DocumentFileId { get; init; }
}
