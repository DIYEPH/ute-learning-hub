using MediatR;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Queries.GetPendingFilesCount;

public class GetPendingFilesCountHandler : IRequestHandler<GetPendingFilesCountQuery, int>
{
    private readonly IDocumentRepository _documentRepository;

    public GetPendingFilesCountHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<int> Handle(GetPendingFilesCountQuery request, CancellationToken cancellationToken)
    {
        return await _documentRepository.GetPendingFilesCountAsync(cancellationToken);
    }
}
