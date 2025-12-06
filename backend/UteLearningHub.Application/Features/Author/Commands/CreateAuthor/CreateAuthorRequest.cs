namespace UteLearningHub.Application.Features.Author.Commands.CreateAuthor;

public record CreateAuthorRequest
{
    public string FullName { get; init; } = default!;
    public string? Description { get; init; }
}
