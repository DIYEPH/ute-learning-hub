using MediatR;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand : IRequest
{
    public Guid ConversationId { get; init; }
    public Guid MemberId { get; init; }
    public ConversationMemberRoleType RoleType { get; init; }
}

