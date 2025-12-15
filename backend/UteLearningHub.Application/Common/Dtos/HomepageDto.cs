namespace UteLearningHub.Application.Common.Dtos;

public record HomepageDto
{
    public IList<DocumentDto> LatestDocuments { get; init; } = [];
    public IList<DocumentDto> PopularDocuments { get; init; } = [];
    public IList<SubjectWithDocsDto> TopSubjects { get; init; } = [];
}

public record SubjectWithDocsDto
{
    public Guid SubjectId { get; init; }
    public string SubjectName { get; init; } = default!;
    public string? SubjectCode { get; init; }
    public IList<DocumentDto> Documents { get; init; } = [];
}
