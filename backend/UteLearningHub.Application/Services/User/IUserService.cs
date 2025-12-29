using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.User.Commands.UpdateUser;
using UteLearningHub.Application.Features.User.Queries.GetUsers;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.User;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<TrustLever?> GetTrustLevelAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<Guid>> GetAllActiveUserIdsAsync(CancellationToken cancellationToken = default);
    Task<IList<Guid>> ValidateUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserDto>> GetUsersAsync(GetUsersRequest request, CancellationToken cancellationToken = default);
    Task BanUserAsync(Guid userId, DateTimeOffset? banUntil, CancellationToken cancellationToken = default);
    Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateTrustScoreAsync(Guid userId, int trustScore, string? reason, Guid? entityId = null, TrustEntityType? entityType = null, CancellationToken cancellationToken = default);
    Task<IList<UserTrustHistoryDto>> GetUserTrustHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
}
