using MediatR;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

public class UpdateDocumentProgressCommandHandler(IDocumentProgressService documentProgressService) : IRequestHandler<UpdateDocumentProgressCommand>
{
    private readonly IDocumentProgressService _documentProgressService = documentProgressService;

    public async Task Handle(UpdateDocumentProgressCommand request, CancellationToken cancellationToken)
    {
        await _documentProgressService.UpdateAsync(request, cancellationToken);
    }
}

