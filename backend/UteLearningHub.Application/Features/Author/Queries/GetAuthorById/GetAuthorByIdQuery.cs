using MediatR;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public record AuthorDetailDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int DocumentCount { get; init; }
}

public record GetAuthorByIdQuery : GetAuthorByIdRequest, IRequest<AuthorDetailDto>;
