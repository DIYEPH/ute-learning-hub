using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentRequest
{
    public Guid Id { get; init; }
    public string? DocumentName { get; init; }
    public string? Description { get; init; }
    public string? AuthorName { get; init; }
    public string? DescriptionAuthor { get; init; }
    public Guid? SubjectId { get; init; }
    public Guid? TypeId { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public bool? IsDownload { get; init; }
    public VisibilityStatus? Visibility { get; init; }
}