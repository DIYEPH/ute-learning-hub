using MediatR;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand : UpdateMemberRoleCommandRequest, IRequest
{
    public Guid ConversationId { get; init; }
    public Guid MemberId { get; init; }
}
