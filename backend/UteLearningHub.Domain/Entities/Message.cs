using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class Message : SoftDeletableEntity<Guid>, IAggregateRoot, IAuditable
{
    public Guid ConversationId { get; set; }
    public Guid? ParentId { get; set; }
    public string Content { get; set; } = default!;
    public bool IsEdit { get; set; }
    public bool IsPined { get; set; }
    public MessageType? Type { get; set; }
    public Conversation Conversation { get; set; } = default!;
    public ICollection<Message> Childrens { get; set; } = [];
    public ICollection<MessageFile> MessageFiles { get; set; } = [];

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

}
