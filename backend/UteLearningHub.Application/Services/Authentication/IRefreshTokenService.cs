namespace UteLearningHub.Application.Services.Authentication;

public interface IRefreshTokenService
{
    Task<string> GenerateAndSaveRefreshTokenAsync(Guid userId, string sessionId);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken, string sessionId);
    Task RevokeRefreshTokenAsync(Guid userId, string? sessionId);
}
