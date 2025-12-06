using MediatR;

namespace UteLearningHub.Application.Features.Author.Commands.DeleteAuthor;

public record DeleteAuthorCommand : IRequest
{
    public Guid Id { get; init; }
}
