namespace UteLearningHub.Application.Features.Auth.Commands.CompleteAccountSetup;

public record CompleteAccountSetupResponse
{
    public bool Success { get; init; }
    public IEnumerable<string> Errors { get; init; } = [];
    public string? Username { get; init; }
    public bool PasswordSet { get; init; }
}
