namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;

public record UpdateDocumentFileCommandRequest
{
    public string? Title { get; init; }
    public int? Order { get; init; }
    public Guid? CoverFileId { get; init; }
}
