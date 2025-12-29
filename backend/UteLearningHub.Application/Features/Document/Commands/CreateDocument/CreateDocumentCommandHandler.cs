using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public class CreateDocumentCommandHandler(IDocumentService documentService) : IRequestHandler<CreateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentService _documentService = documentService;

    public async Task<DocumentDetailDto> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        return await _documentService.CreateAsync(request, ct);
    }
}
