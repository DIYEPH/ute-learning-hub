using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentCommandRequest
{
    public string? DocumentName { get; init; }
    public string? Description { get; init; }
    public Guid? SubjectId { get; init; }
    public Guid TypeId { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public IList<Guid>? AuthorIds { get; init; }
    public IList<AuthorInput>? Authors { get; init; }
    public VisibilityStatus? Visibility { get; init; }
    public IList<Guid>? FileIdsToRemove { get; init; }
    public Guid? CoverFileId { get; init; }
}
