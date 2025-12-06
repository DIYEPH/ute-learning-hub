namespace UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;

public record UpdateAuthorRequest
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string? Description { get; init; }
}
