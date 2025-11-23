using MediatR;

namespace UteLearningHub.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest<Unit>;
