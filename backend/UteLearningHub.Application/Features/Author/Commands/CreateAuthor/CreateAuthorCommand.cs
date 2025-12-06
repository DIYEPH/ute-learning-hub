using MediatR;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

namespace UteLearningHub.Application.Features.Author.Commands.CreateAuthor;

public record CreateAuthorCommand : CreateAuthorRequest, IRequest<AuthorDetailDto>;
