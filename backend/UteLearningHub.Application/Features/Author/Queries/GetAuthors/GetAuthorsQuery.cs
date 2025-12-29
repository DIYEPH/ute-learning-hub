using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Queries.GetAuthors;

public record GetAuthorsQuery : GetAuthorsRequest, IRequest<PagedResponse<AuthorDetailDto>>;
