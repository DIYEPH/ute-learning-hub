namespace UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;

public record LoginWithMicrosoftRequest
{
    public string IdToken { get; set; } = default!;
}
