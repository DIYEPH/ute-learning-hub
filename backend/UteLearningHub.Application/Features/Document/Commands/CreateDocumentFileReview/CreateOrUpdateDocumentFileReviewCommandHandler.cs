using MediatR;
using UteLearningHub.Application.Services.Document;

namespace UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;

public class CreateOrUpdateDocumentFileReviewCommandHandler(IDocumentFileService documentFileService) : IRequestHandler<CreateOrUpdateDocumentFileReviewCommand>
{
    private readonly IDocumentFileService _documentFileService = documentFileService;

    public async Task Handle(CreateOrUpdateDocumentFileReviewCommand request, CancellationToken ct)
    {
        await _documentFileService.CreateOrUpdateDocumentFile(request, ct);
    }
}

