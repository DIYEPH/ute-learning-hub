using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = default!;
    public string? Username { get; init; }
    public string FullName { get; init; } = default!;
    public bool EmailConfirmed { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Introduction { get; init; }
    public int TrustScore { get; init; }
    public string TrustLevel { get; init; } = default!;
    public Gender Gender { get; init; }
    public bool IsSuggest { get; init; }
    public IList<string> Roles { get; init; } = [];
    public MajorDetailDto? Major { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    public Guid? DeletedById { get; init; }
    public bool LockoutEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
}
