using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public record AddDocumentFileCommand : IRequest<DocumentDetailDto>
{
    public Guid DocumentId { get; init; }
    public string? Title { get; init; }
    public Guid FileId { get; init; }
    public Guid? CoverFileId { get; init; }
}
