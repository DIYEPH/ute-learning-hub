namespace UteLearningHub.Application.Common.Dtos;

public record AuthorDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Description { get; init; } = default!;
}
