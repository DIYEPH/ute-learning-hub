namespace UteLearningHub.Application.Services.Authentication;

public interface IMicrosoftTokenValidator
{
    Task<MicrosoftUserInfo?> ValidateTokenAsync(string idToken, CancellationToken ct);
}
public record MicrosoftUserInfo(
    string Email,
    string Name,
    string? GivenName,
    string? FamilyName,
    string MicrosoftUserId
);
