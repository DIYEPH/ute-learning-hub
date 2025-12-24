using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.RespondToInvitation;

public record RespondToInvitationCommand : IRequest<bool>
{
    public Guid InvitationId { get; init; }
    public bool Accept { get; init; }
    public string? Note { get; init; }
}
