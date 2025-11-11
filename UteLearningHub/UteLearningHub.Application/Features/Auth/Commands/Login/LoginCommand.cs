using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.Login;

public record LoginCommand : LoginRequest, IRequest<LoginResponse>
{
}
