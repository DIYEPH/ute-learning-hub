using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Profile;

namespace UteLearningHub.Application.Features.Account.Queries.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, ProfileDetailDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfileService _profileService;
    public GetProfileHandler(ICurrentUserService currentUserService, IProfileService profileService)
    {
        _currentUserService = currentUserService;
        _profileService = profileService;
    }
    public async Task<ProfileDetailDto> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var targetUserId = request.UserId ?? _currentUserService.UserId;
        var isAdmin = _currentUserService.IsInRole("Admin");
        var isOwn = (_currentUserService.UserId == targetUserId);
        return await _profileService.GetProfileByIdAsync(targetUserId, (isAdmin || isOwn), ct);
    }
}
