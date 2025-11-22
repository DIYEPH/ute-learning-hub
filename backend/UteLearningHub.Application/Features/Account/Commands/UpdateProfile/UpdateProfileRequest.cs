using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Queries.GetProfile;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public record UpdateProfileRequest
{
    public string? FullName { get; init; }
    public string? Introduction { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid? MajorId { get; init; }
    public Gender? Gender { get; init; }
}
