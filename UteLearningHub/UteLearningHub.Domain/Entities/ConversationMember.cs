using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationMember : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public bool IsMuted { get; set; }
    public ConversationMemberRoleType ConversationMemberRoleType { get; set; } 
    public Conversation Conversation { get; set; } = default!;
}
