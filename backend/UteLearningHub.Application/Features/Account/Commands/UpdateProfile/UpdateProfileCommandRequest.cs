using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileCommandRequest
{
    public string? Introduction { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid? MajorId { get; init; }
    public Gender? Gender { get; init; }
}
