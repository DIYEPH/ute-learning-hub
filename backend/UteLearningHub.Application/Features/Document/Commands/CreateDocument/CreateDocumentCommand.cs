using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public record CreateDocumentCommand : CreateDocumentRequest, IRequest<DocumentDetailDto>
{
    public IList<IFormFile>? Files { get; init; }
}
