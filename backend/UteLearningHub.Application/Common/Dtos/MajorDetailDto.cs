namespace UteLearningHub.Application.Common.Dtos;

public record MajorDetailDto
{
    public Guid Id { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
    public FacultyDto Faculty { get; init; } = default!;
    public int SubjectCount { get; init; }
}
