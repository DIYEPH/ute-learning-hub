using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.User.Commands.UpdateUser;

public record UpdateUserRequest
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Username { get; init; }
    public string? Introduction { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid? MajorId { get; init; }
    public Gender? Gender { get; init; }
    public bool? EmailConfirmed { get; init; }
    public IList<string>? Roles { get; init; }
}
