using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public class GetDocumentByIdHandler(IDocumentService documentService) : IRequestHandler<GetDocumentByIdQuery, DocumentDetailDto>
{
    private readonly IDocumentService _documentService = documentService;

    public async Task<DocumentDetailDto> Handle(GetDocumentByIdQuery request, CancellationToken ct)
    {
        return await _documentService.GetDocumentByIdAsync(request.Id, ct);
    }
}
