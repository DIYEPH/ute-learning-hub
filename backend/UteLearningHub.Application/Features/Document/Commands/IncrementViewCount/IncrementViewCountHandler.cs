using MediatR;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Domain.Repositories;

namespace UteLearningHub.Application.Features.Document.Commands.IncrementViewCount;

public class IncrementViewCountHandler : IRequestHandler<IncrementViewCountCommand, Unit>
{
    private readonly IDocumentRepository _docRepo;

    public IncrementViewCountHandler(IDocumentRepository docRepo)
    {
        _docRepo = docRepo;
    }

    public async Task<Unit> Handle(IncrementViewCountCommand req, CancellationToken ct)
    {
        var docFile = await _docRepo.GetDocumentFileByIdAsync(req.DocumentFileId, disableTracking: false, ct);
        if (docFile == null)
            throw new NotFoundException($"DocumentFile with id {req.DocumentFileId} not found");

        docFile.ViewCount++;

        await _docRepo.UnitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
