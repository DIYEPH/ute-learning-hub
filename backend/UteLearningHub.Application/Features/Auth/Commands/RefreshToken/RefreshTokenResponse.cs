namespace UteLearningHub.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenResponse
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
}
