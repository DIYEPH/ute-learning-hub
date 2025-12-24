using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Domain.Exceptions;
using UteLearningHub.Persistence;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Infrastructure.Services.Authentication;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _dbContext;
    private const string RefreshTokenProvider = "RefreshTokenProvider";
    private const string RefreshTokenPurpose = "RefreshToken";

    public RefreshTokenService(
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, string sessionId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new NotFoundException();
        var refreshToken = _jwtTokenService.GenerateRefreshToken(userId, sessionId);
        var tokenKey = $"{RefreshTokenPurpose}_{sessionId}";
        await _userManager.SetAuthenticationTokenAsync(user, RefreshTokenProvider, tokenKey, refreshToken);
        return refreshToken;
    }

    public async Task RevokeRefreshTokenAsync(Guid userId, string? sessionId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return;

        if (string.IsNullOrEmpty(sessionId))
        {
            // Revoke all sessions: Delete all refresh tokens for this user from database
            var tokensToDelete = await _dbContext.UserTokens
                .Where(t => t.UserId == userId &&
                           t.LoginProvider == RefreshTokenProvider &&
                           t.Name.StartsWith(RefreshTokenPurpose))
                .ToListAsync(cancellationToken);

            if (tokensToDelete.Any())
            {
                _dbContext.UserTokens.RemoveRange(tokensToDelete);
                try
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Token was already deleted by another request, ignore
                }
            }
        }
        else
        {
            var tokenKey = $"{RefreshTokenPurpose}_{sessionId}";
            try
            {
                await _userManager.RemoveAuthenticationTokenAsync(user, RefreshTokenProvider, tokenKey);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Token was already deleted or doesn't exist, ignore
            }
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
}
