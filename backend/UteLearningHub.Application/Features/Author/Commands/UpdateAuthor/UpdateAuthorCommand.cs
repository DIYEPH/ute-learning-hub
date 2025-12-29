using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public record UpdateAuthorCommand : UpdateAuthorCommandRequest, IRequest<AuthorDetailDto>
{
    public Guid Id { get; init; }
}
