namespace UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;

public record LoginWithMicrosoftResponse
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Username { get; set; } = default!;
    public string? AvatarUrl { get; set; } = default!;
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public bool RequiresSetup { get; set; }
}
