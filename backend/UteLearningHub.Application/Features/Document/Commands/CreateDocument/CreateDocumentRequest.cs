using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public record CreateDocumentRequest
{
    public string DocumentName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Guid? SubjectId { get; init; }
    public Guid TypeId { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; } 
    public IList<string>? AuthorNames { get; init; }
    public bool IsDownload { get; init; } = true;
    public VisibilityStatus Visibility { get; init; } = VisibilityStatus.Internal;
}
