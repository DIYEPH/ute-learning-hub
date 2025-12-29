using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public record GetAuthorByIdQuery : IRequest<AuthorDetailDto>
{
    public Guid Id { get; init; }
}
