using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public record AuthorListDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Description { get; init; } = default!;
}

public record GetAuthorsQuery : GetAuthorsRequest, IRequest<PagedResponse<AuthorListDto>>;
