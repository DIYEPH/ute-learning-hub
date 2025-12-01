using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public record AddDocumentFileCommand : AddDocumentFileRequest, IRequest<DocumentDetailDto>
{
    public IFormFile File { get; init; } = default!;
    public IFormFile? CoverFile { get; init; }
}


