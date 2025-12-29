namespace UteLearningHub.Application.Common.Dtos;

public record TagDto
{
    public Guid Id { get; init; }
    public string TagName { get; init; } = default!;
}
