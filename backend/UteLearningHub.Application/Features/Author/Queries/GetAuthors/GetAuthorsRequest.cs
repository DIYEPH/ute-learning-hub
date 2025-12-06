using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public record GetAuthorsRequest : PagedRequest
{
    public string? SearchTerm { get; init; }
    public bool? IsDeleted { get; init; }
}

