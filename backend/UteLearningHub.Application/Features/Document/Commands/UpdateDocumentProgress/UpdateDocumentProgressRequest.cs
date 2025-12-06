namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public record UpdateDocumentProgressRequest
{
    public Guid DocumentFileId { get; init; }
    public int LastPage { get; init; }
}

