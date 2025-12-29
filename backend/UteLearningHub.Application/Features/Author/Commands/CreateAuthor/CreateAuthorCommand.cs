using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Author.Commands.CreateAuthor;

public record CreateAuthorCommand : IRequest<AuthorDetailDto>
{
    public string FullName { get; init; } = default!;
    public string? Description { get; init; }
}
