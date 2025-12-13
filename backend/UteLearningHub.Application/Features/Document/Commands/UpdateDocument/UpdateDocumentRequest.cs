using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentRequest
{
    public Guid Id { get; init; }
    public string? DocumentName { get; init; }
    public string? Description { get; init; }
    public Guid? SubjectId { get; init; }
    public Guid? TypeId { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<Guid>? AuthorIds { get; init; }
    public IList<AuthorInput>? Authors { get; init; }
    public VisibilityStatus? Visibility { get; init; }
    public IList<Guid>? FileIdsToRemove { get; init; }
    public Guid? CoverFileId { get; init; }
}

public record AuthorInput
{
    public string FullName { get; init; } = string.Empty;
    public string? Description { get; init; }
}