using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.SendInvitation;

public record SendInvitationCommand : IRequest<SendInvitationResponse>
{
    public Guid ConversationId { get; init; }
    public Guid InvitedUserId { get; init; }
    public string? Message { get; init; }
}

public record SendInvitationResponse
{
    public Guid InvitationId { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
}
