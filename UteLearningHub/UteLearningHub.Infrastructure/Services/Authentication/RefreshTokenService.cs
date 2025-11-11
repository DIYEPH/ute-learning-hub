using Microsoft.AspNetCore.Identity;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Infrastructure.Services.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private const string RefreshTokenProvider = "RefreshTokenProvider";
    private const string RefreshTokenPurpose = "RefreshToken";
    public RefreshTokenService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }
    public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, string sessionId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException();
        var refreshToken = _jwtTokenService.GenerateRefreshToken(userId, sessionId);
        var tokenKey = $"{RefreshTokenPurpose}_{sessionId}";
        await _userManager.SetAuthenticationTokenAsync(user, RefreshTokenProvider, tokenKey, refreshToken);
        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string? sessionId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return;
        if (string.IsNullOrEmpty(sessionId))
            //await RevokeAllSessionsAsync(user);
            throw new Exception("DeleteAll");
        else
        {
            var tokenKey = $"{RefreshTokenPurpose}_{sessionId}";
            await _userManager.RemoveAuthenticationTokenAsync(user, RefreshTokenProvider, tokenKey);
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, string sessionId)
    {
        var (isValid, tokenUserId, tokenSessionId) = _jwtTokenService.ValidateRefreshToken(refreshToken);
        if (!isValid || tokenUserId != userId || tokenSessionId != sessionId)
            return false;
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException();
        var tokenKey = $"{RefreshTokenPurpose}_{sessionId}";
        var storedToken = await _userManager.GetAuthenticationTokenAsync(
            user,
            RefreshTokenProvider,
            tokenKey
        );
        return storedToken == refreshToken;

    }
    //private async Task RevokeAllSessionsAsync(AppUser user)
    //{
    //    try
    //    {
    //        var userTokens = await 
    //    }
    //}
}
