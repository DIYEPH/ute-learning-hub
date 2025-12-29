using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;

namespace UteLearningHub.Application.Services.Document;

public interface IDocumentFileService
{
    public Task IncrementViewCountAsync(Guid documentFile, CancellationToken cancellationToken);
    public Task CreateOrUpdateDocumentFile(CreateOrUpdateDocumentFileReviewCommand request, CancellationToken cancellationToken);
    Task<DocumentDetailDto> CreateAsync(AddDocumentFileCommand request, CancellationToken ct);
    Task<DocumentDetailDto> UpdateAsync(UpdateDocumentCommand request, CancellationToken ct);
    Task SoftDeleteAsync(Guid documentFileId, CancellationToken ct);
}
