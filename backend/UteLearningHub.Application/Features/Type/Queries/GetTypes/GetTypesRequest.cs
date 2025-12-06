using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Type.Queries.GetTypes;

public record GetTypesRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public bool? IsDeleted { get; init; }
}
