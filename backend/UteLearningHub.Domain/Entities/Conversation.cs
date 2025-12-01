using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Conversation : BaseEntity<Guid>, IAggregateRoot, IAuditable
{
    public Guid? SubjectId { get; set; }
    public Guid? LastMessage { get; set; }
    public string ConversationName { get; set; } = default!;
    public bool IsSuggestedByAI { get; set; }
    public bool IsAllowMemberPin { get; set; }
    public ConversitionType ConversationType { get; set; }
    public ConversationStatus ConversationStatus { get; set; } = ConversationStatus.Active;
    public Subject Subject { get; set; } = default!;
    public ICollection<ConversationTag> ConversationTags { get; set; } = [];
    public ICollection<ConversationMember> Members { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<ConversationJoinRequest> ConversationJoinRequests { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
