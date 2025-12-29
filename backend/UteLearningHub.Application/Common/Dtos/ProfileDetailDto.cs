using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record ProfileDetailDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string? Username { get; init; }
    public string FullName { get; init; } = default!;
    public bool? EmailConfirmed { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Introduction { get; init; }
    public int TrustScore { get; init; }
    public TrustLever TrustLevel { get; init; }
    public Gender? Gender { get; init; }
    public IList<string> Roles { get; init; } = [];
    public Guid? MajorId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
    public bool? IsLocked { get; init; }
}
