using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;

public record UpdateDocumentFileCommand : UpdateDocumentFileCommandRequest, IRequest<DocumentDetailDto>
{
    public Guid DocumentId { get; init; }
    public Guid DocumentFileId { get; init; }
}


