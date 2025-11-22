using UteLearningHub.Application.Features.Account.Queries.GetProfile;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.User;

public interface IUserService
{
    Task<GetProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TrustLever?> GetTrustLevelAsync(Guid userId, CancellationToken cancellationToken = default);
}
