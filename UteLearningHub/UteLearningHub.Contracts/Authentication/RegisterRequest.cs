namespace UteLearningHub.Contracts.Authentication;

public record RegisterRequest(string Email, string Password, string FullName);