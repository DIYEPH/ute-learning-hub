using UteLearningHub.Application.Features.Account.Queries.GetProfile;

namespace UteLearningHub.Application.Services.User;

public interface IUserService
{
    Task<GetProfileResponse?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
