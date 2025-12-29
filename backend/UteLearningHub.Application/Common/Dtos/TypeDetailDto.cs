namespace UteLearningHub.Application.Common.Dtos;

public record TypeDetailDto
{
    public Guid Id { get; init; }
    public string TypeName { get; init; } = default!;
    public int DocumentCount { get; init; }

    //track
    public bool? IsDeleted { get; init; }
    public Guid? DeletedById { get; init; }
    public Guid? CreatedById { get; init; }
    public Guid? UpdatedById { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
