using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocumentFile;

public record DeleteDocumentFileCommand : IRequest
{
    public Guid DocumentFileId { get; init; }
}


