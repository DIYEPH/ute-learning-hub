using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.ChangeUsername;

public record ChangeUsernameCommand : IRequest<Unit>
{
    public string NewUsername { get; init; } = default!;
}


