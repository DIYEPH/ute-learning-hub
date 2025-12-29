using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.CreateDocument;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Features.Document.Queries.GetDocuments;

namespace UteLearningHub.Application.Services.Document;

public interface IDocumentService
{
    Task<DocumentDetailDto> GetDocumentByIdAsync(Guid id, CancellationToken ct);

    Task<PagedResponse<DocumentDetailDto>> GetDocumentsAsync(GetDocumentsQuery request, CancellationToken ct);

    Task<DocumentDetailDto> CreateAsync(CreateDocumentCommand request, CancellationToken ct);

    Task<DocumentDetailDto> UpdateAsync(UpdateDocumentCommand request, CancellationToken ct);

    Task SoftDeleteAsync(Guid documentId, CancellationToken ct);
}
