using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Profile;

namespace UteLearningHub.Application.Features.Account.Queries.GetUserStats;

public class GetUserStatsHandler : IRequestHandler<GetUserStatsQuery, UserStatsDto>
{
    private readonly IProfileService _profileService;
    private readonly ICurrentUserService _currentUserService;

    public GetUserStatsHandler(IProfileService profileService, ICurrentUserService currentUserService)
    {
        _profileService = profileService;
        _currentUserService = currentUserService;
    }

    public async Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
    {
        var targetUserId = request.UserId ?? _currentUserService.UserId;

        if (!targetUserId.HasValue)
            return new UserStatsDto { Uploads = 0, Upvotes = 0, Comments = 0 };

        return await _profileService.GetUserStatsAsync(targetUserId.Value, cancellationToken);
    }
}
