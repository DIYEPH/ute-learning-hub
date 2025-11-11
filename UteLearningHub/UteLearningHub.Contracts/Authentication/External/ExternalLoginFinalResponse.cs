namespace UteLearningHub.Contracts.Authentication.External;
public record ExternalLoginFinalResponse(Guid Id, string Email, string FullName, string? AvatarUrl, string AccessToken);
