namespace UteLearningHub.Application.Common.Dtos;

public record TypeDetailDto
{
    public Guid Id { get; init; }
    public string TypeName { get; init; } = default!;
    public int DocumentCount { get; init; }
}
