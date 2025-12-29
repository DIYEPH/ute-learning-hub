namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public record UpdateDocumentProgressCommandRequest
{
    public int LastPage { get; init; }
}
