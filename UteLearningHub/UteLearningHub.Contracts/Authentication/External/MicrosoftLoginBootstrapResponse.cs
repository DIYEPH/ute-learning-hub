namespace UteLearningHub.Contracts.Authentication.External;

public record MicrosoftLoginBootstrapResponse(Guid Id, string Email, string FullName, bool RequiresLocalPassword);
