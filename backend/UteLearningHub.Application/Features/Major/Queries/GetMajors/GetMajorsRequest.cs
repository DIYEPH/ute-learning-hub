using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Major.Queries.GetMajors;

public record GetMajorsRequest : PagedRequest
{
    public Guid? FacultyId { get; init; }
    public string? SearchTerm { get; init; }
}
