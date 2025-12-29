using MediatR;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.DeleteDocumentFile;

public class DeleteDocumentFileCommandHandler(IDocumentFileService documentFileService) : IRequestHandler<DeleteDocumentFileCommand>
{
    private readonly IDocumentFileService _documentFileService = documentFileService;

    public async Task Handle(DeleteDocumentFileCommand request, CancellationToken ct)
    {
        await _documentFileService.SoftDeleteAsync(request.DocumentFileId, ct);
    }
}
