using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public record CreateDocumentCommand : CreateDocumentRequest, IRequest<DocumentDetailDto>
{
    // 1 document = 1 file
    public IFormFile? File { get; init; }
}
