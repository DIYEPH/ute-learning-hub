using MediatR;

namespace UteLearningHub.Application.Features.User.Commands.UnbanUser;

public record UnbanUserCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
}
