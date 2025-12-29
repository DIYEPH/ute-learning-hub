namespace UteLearningHub.Application.Common.Dtos;

public record SubjectDetailDto
{
    public Guid Id { get; init; }
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
    public IList<MajorDto> Majors { get; init; } = [];
    public int? DocumentCount { get; init; }

    //track
    public bool? IsDeleted { get; init; }
    public Guid? DeletedById { get; init; }
    public Guid? CreatedById { get; init; }
    public Guid? UpdatedById { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
