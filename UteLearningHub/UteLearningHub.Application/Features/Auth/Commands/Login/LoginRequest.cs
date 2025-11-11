namespace UteLearningHub.Application.Features.Auth.Commands.Login;

public record LoginRequest
{
    public string EmailOrUsername { get; set; } = default!;
    public string Password { get; set; } = default!;
}
