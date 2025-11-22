using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Common.Dtos;

public record SubjectDetailDto
{
    public Guid Id { get; init; }
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
    public MajorDto Major { get; init; } = default!;
    public int DocumentCount { get; init; }
}
