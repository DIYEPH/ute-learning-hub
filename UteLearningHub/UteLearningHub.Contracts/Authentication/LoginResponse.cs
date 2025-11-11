namespace UteLearningHub.Contracts.Authentication;

public record LoginResponse(Guid Id, string Email, string FullName, string? AvatarUrl, string AccessToken);