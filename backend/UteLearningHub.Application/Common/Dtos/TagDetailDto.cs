
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record TagDetailDto
{
    public Guid Id { get; init; }
    public string TagName { get; init; } = default!;
    public int DocumentCount { get; init; }

    //track
    public ContentStatus Status { get; init; }
    public bool? IsDeleted { get; init; }
    public Guid? DeletedById { get; init; }
    public Guid? CreatedById { get; init; }
    public Guid? UpdatedById { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}


