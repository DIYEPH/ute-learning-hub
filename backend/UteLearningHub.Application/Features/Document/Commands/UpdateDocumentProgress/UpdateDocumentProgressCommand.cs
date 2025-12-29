using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public record UpdateDocumentProgressCommand : UpdateDocumentProgressCommandRequest, IRequest
{
    public Guid DocumentFileId { get; init; }
}

