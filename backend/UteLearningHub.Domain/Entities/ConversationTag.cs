namespace UteLearningHub.Domain.Entities;

public class ConversationTag
{
    public Guid TagId { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = default!;
    public Tag Tag { get; set; } = default!;
}

