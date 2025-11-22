using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Account.Commands.UpdateProfile;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.User;

public interface IUserService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TrustLever?> GetTrustLevelAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<IList<Guid>> GetAllActiveUserIdsAsync(CancellationToken cancellationToken = default);
    Task<IList<Guid>> ValidateUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
}
