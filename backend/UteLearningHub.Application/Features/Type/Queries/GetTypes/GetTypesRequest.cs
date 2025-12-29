using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTypes;

public record GetTypesRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public bool? IsDeleted { get; init; }
}