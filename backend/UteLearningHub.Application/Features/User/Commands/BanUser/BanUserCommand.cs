using MediatR;

namespace UteLearningHub.Application.Features.User.Commands.BanUser;

public record BanUserCommand : IRequest<Unit>
{
    public Guid UserId { get; init; }
    public DateTimeOffset? BanUntil { get; init; } // null = ban vĩnh viễn
}
