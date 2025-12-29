using UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;

namespace UteLearningHub.Application.Services.Document;

public interface IDocumentProgressService
{
    Task UpdateAsync(UpdateDocumentProgressCommand request, CancellationToken ct);
}
