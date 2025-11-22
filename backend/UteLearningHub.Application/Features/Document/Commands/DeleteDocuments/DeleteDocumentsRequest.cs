namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public record DeleteDocumentsRequest
{
    public Guid Id { get; init; }
}
