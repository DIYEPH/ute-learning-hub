using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public record GetSubjectsRequest : PagedRequest
{
    public List<Guid>? MajorIds { get; init; } = [];
    public string? SearchTerm { get; init; }
    public bool? IsDeleted { get; init; }
}
