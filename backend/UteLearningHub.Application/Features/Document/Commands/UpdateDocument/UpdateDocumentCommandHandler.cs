using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler(IDocumentService documentService) : IRequestHandler<UpdateDocumentCommand, DocumentDetailDto>
{
    private readonly IDocumentService _documentService = documentService;

    public async Task<DocumentDetailDto> Handle(UpdateDocumentCommand request, CancellationToken ct)
    {
        return await _documentService.UpdateAsync(request, ct);
    }
}
