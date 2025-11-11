using MediatR;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityService _identityService;
    public LoginCommandHandler(IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, IIdentityService identityService)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _identityService = identityService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        //1. Check Email
        var user = (request.EmailOrUsername.Contains('@')
            ? await _identityService.FindByEmailAsync(request.EmailOrUsername)
            : await _identityService.FindByEmailAsync(request.EmailOrUsername)) ?? throw new UnauthorizedException();

        var sessionId = Guid.NewGuid().ToString();
        var roles = await _identityService.GetRolesAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.UserName, roles, sessionId);
        var refreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, sessionId);
        return new LoginResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email!,
            FullName = user.FullName,
            Username = user.UserName,
            AvatarUrl = user.AvatarUrl,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
