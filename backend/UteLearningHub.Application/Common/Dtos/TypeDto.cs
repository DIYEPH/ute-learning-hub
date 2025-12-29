namespace UteLearningHub.Application.Common.Dtos;

public record TypeDto
{
    public Guid Id { get; init; }
    public string TypeName { get; init; } = default!;
}
