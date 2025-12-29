using MediatR;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.IncrementViewCount;

public class IncrementViewCountHandler(IDocumentFileService documentFileService) : IRequestHandler<IncrementViewCountCommand>
{
    private readonly IDocumentFileService _documentFileService = documentFileService;

    public async Task Handle(IncrementViewCountCommand req, CancellationToken ct)
    {
        await _documentFileService.IncrementViewCountAsync(req.DocumentFileId, ct);
    }
}
