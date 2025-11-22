using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public record GetSubjectsRequest : PagedRequest
{
    public Guid? MajorId { get; init; }
    public string? SearchTerm { get; init; }
}
