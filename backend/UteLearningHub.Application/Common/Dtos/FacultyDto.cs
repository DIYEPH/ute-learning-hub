namespace UteLearningHub.Application.Common.Dtos;

public record FacultyDto
{
    public Guid Id { get; init; }
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
    public string Logo { get; init; } = default!;
}
