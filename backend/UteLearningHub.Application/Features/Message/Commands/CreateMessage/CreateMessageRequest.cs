namespace UteLearningHub.Application.Features.Message.Commands.CreateMessage;

public record CreateMessageRequest
{
    public Guid ConversationId { get; init; }
    public Guid? ParentId { get; init; } // Để reply message
    public string Content { get; init; } = default!;
    public IList<Guid>? FileIds { get; init; } // IDs của files đã upload trước
}