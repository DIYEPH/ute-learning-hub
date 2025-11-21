using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Queries.GetDocuments;

public record GetDocumentsRequest : PagedRequest
{
    public Guid? SubjectId { get; init; }
    public Guid? TypeId { get; init; }
    public Guid? TagId { get; init; }
    public Guid? MajorId { get; init; }
    public string? SearchTerm { get; init; }
    public VisibilityStatus? Visibility { get; init; }
    public ReviewStatus? ReviewStatus { get; init; }
    public bool? IsDownload { get; init; }
    public string? SortBy { get; init; } // "name", "createdAt", "authorName"
    public bool SortDescending { get; init; } = true;
}
