using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Account.Queries.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, GetProfileResponse>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    public GetProfileHandler(ICurrentUserService currentUserService, IIdentityService identityService, IUserService userService)
    {
        _currentUserService = currentUserService;
        _identityService = identityService;
        _userService = userService;
    }
    public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        Guid userId;

        if (request.UserId.HasValue)
            userId = request.UserId.Value;
        else
        {
            if (!_currentUserService.IsAuthenticated)
                throw new UnauthorizedException();

            userId = _currentUserService.UserId ?? throw new UnauthorizedException();
        }

        var user = await _identityService.FindByIdAsync(userId);

        if (user == null)
            throw new NotFoundException("User " + userId);

        var profile = await _userService.GetProfileAsync(userId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("User " + userId);

        return profile;
    }
}
