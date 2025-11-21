namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocument;

public record DeleteDocumentRequest
{
    public Guid Id { get; set; }
}
