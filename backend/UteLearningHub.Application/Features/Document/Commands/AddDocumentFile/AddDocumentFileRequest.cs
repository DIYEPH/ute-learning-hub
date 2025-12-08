namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public record AddDocumentFileRequest
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public int? Order { get; init; }
    public bool IsPrimary { get; init; } = false;
    public int? TotalPages { get; init; }
    public Guid FileId { get; init; }
    public Guid? CoverFileId { get; init; }
}


