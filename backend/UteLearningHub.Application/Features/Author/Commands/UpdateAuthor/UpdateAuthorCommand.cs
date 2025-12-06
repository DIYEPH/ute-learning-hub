using MediatR;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public record UpdateAuthorCommand : UpdateAuthorRequest, IRequest<AuthorDetailDto>;
