namespace UteLearningHub.Application.Common.Dtos;

public record FacultyDetailDto
{
    public Guid Id { get; init; }
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
    public string? Logo { get; init; } = default!;
    public int? MajorCount { get; init; }
    //track
    public bool? IsDeleted { get; init; }
    public Guid? DeletedById { get; init; }
    public Guid? CreatedById { get; init; }
    public Guid? UpdatedById { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
