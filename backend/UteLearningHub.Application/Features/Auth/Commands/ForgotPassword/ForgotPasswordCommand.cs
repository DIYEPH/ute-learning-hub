using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest<Unit>
{
    public string Email { get; init; } = default!;
}

