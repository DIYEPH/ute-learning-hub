using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Message : BaseEntity<Guid>, IAggregateRoot, IAuditable<Guid>
{
    public Guid ConversationId { get; set; }
    public Guid ParentId { get; set; }
    public string Content { get; set; } = default!;
    public bool IsEdit { get; set; }
    public bool IsPined { get; set; }
    public Conversation Conversation { get; set; } = default!;
    public ICollection<MessageFile> MessageFiles { get; set; } = [];

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

}
