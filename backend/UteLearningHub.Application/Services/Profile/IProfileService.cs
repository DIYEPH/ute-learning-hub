using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

namespace UteLearningHub.Application.Services.Profile;

public interface IProfileService
{
    Task<ProfileDetailDto> GetProfileByIdAsync(Guid? userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<ProfileDetailDto> UpdateAsync(Guid actorId, UpdateProfileCommand request, CancellationToken cancellationToken = default);
    Task<UserStatsDto> GetUserStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}

