namespace UteLearningHub.Application.Common.Dtos;

public record SubjectDto
{
    public Guid Id { get; init; }
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
}
