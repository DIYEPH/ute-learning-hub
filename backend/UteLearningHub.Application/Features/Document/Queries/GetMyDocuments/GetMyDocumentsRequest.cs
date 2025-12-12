using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Document.Queries.GetMyDocuments;

public record GetMyDocumentsRequest : PagedRequest
{
    public Guid? SubjectId { get; init; }
    public Guid? TypeId { get; init; }
    public Guid? TagId { get; init; }
    public string? SearchTerm { get; init; }
    public VisibilityStatus? Visibility { get; init; }
    public ContentStatus? FileStatus { get; init; }
    public string? SortBy { get; init; } // "name", "createdAt", "authorName"
    public bool SortDescending { get; init; } = true;
}


