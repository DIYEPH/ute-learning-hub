using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;

public class AddDocumentFileCommandHandler(IDocumentFileService documentFileService) : IRequestHandler<AddDocumentFileCommand, DocumentDetailDto>
{
    private readonly IDocumentFileService _documentFileService = documentFileService;

    public async Task<DocumentDetailDto> Handle(AddDocumentFileCommand request, CancellationToken ct)
    {
        return await _documentFileService.CreateAsync(request, ct);
    }
}