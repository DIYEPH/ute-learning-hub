namespace UteLearningHub.Application.Services.Authentication;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, string? userName, IList<string> roles, string sessionId);
    string GenerateRefreshToken(Guid userId, string sessionId);
    (bool, Guid?, string?) ValidateAccessToken(string accessToken);
    (bool, Guid?, string?) ValidateRefreshToken(string refreshToken);
}
