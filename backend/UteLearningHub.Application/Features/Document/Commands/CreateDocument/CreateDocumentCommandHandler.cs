using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocument;

public class CreateDocumentCommandHandler(IDocumentService documentService) : IRequestHandler<CreateDocumentCommand, DocumentDetailDto>
{
    public async Task<DocumentDetailDto> Handle(CreateDocumentCommand request, CancellationToken ct)
    {
        return await documentService.CreateAsync(request, ct);
    }
}
