namespace UteLearningHub.Application.Common.Dtos;

public record MajorDto
{
    public Guid Id { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
    public string? FacultyName { get; init; }
    public string? FacultyCode { get; init; }
}
