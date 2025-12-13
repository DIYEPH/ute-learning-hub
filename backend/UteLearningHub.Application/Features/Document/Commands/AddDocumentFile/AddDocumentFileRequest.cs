namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public record AddDocumentFileRequest
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public Guid FileId { get; init; }
    public Guid? CoverFileId { get; init; }
}


