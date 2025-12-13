using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;

public record UpdateDocumentFileCommand : IRequest<DocumentDetailDto>
{
    public Guid DocumentId { get; init; }
    public Guid DocumentFileId { get; init; }

    public string? Title { get; init; }
    public int? Order { get; init; }
    public Guid? CoverFileId { get; init; }
}


