using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Tag.Queries.GetTags;

public record GetTagsRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
}