namespace UteLearningHub.Application.Common.Dtos;

public record AuthorInput
{
    public string FullName { get; init; } = default!;
    public string? Description { get; init; }
}
