namespace UteLearningHub.Application.Common.Dtos;

public record FacultyDetailDto
{
    public Guid Id { get; init; }
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
    public string Logo { get; init; } = default!;
    public int MajorCount { get; init; }
}