using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommandRequest
{
    public ConversationMemberRoleType RoleType { get; init; }
}
