using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public record CreateDocumentCommand : IRequest<DocumentDetailDto>
{
    public string DocumentName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Guid? SubjectId { get; init; }
    public Guid TypeId { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public IList<Guid>? AuthorIds { get; init; }
    public IList<AuthorInput>? Authors { get; init; }
    public VisibilityStatus Visibility { get; init; } = VisibilityStatus.Internal;
    public Guid? CoverFileId { get; init; }
}
