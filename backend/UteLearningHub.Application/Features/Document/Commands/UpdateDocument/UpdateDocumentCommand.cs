using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public record UpdateDocumentCommand : UpdateDocumentRequest, IRequest<DocumentDetailDto>
{
    public IList<IFormFile>? FilesToAdd { get; init; }
}