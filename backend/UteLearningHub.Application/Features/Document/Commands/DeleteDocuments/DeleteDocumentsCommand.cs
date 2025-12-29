using MediatR;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public record DeleteDocumentsCommand : IRequest
{
    public Guid Id { get; init; }
}
