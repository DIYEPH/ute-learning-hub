using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordCommand : IRequest<Unit>
{
    public string CurrentPassword { get; init; } = default!;
    public string NewPassword { get; init; } = default!;
}


