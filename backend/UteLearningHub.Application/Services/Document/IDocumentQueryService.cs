using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Document;

public interface IDocumentQueryService
{
    Task<DocumentDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
