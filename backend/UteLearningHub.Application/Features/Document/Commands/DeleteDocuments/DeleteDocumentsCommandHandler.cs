using MediatR;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;

public class DeleteDocumentsCommandHandler(IDocumentService documentService) : IRequestHandler<DeleteDocumentsCommand>
{
    private readonly IDocumentService _documentService = documentService;

    public async Task Handle(DeleteDocumentsCommand request, CancellationToken ct)
    {
        await _documentService.SoftDeleteAsync(request.Id, ct);
    }
}