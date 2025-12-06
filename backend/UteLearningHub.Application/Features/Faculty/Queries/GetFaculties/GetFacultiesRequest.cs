using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

public record GetFacultiesRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public bool? IsDeleted { get; init; }
}
