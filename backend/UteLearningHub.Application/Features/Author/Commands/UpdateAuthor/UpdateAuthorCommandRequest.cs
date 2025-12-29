namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public record UpdateAuthorCommandRequest
{
    public string? FullName { get; init; }
    public string? Description { get; init; }
}
