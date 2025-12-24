using MediatR;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IIdentityService identityService)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _identityService = identityService;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate refresh token format and extract claims
        var (isValid, userId, sessionId) = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        
        if (!isValid || !userId.HasValue || string.IsNullOrEmpty(sessionId))
            throw new UnauthorizedException("Invalid refresh token");

        // 2. Validate refresh token exists in database
        var isTokenValid = await _refreshTokenService.ValidateRefreshTokenAsync(userId.Value, request.RefreshToken, sessionId);
        if (!isTokenValid)
            throw new UnauthorizedException("Refresh token expired or revoked");

        // 3. Get user info
        var user = await _identityService.FindByIdAsync(userId.Value);
        if (user == null)
            throw new UnauthorizedException("User not found");

        // 4. Revoke old refresh token
        await _refreshTokenService.RevokeRefreshTokenAsync(userId.Value, sessionId, cancellationToken);

        // 5. Generate new tokens with new session
        var newSessionId = Guid.NewGuid().ToString();
        var roles = await _identityService.GetRolesAsync(userId.Value);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(userId.Value, user.Email, user.UserName, roles, newSessionId);
        var newRefreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(userId.Value, newSessionId);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }
}
