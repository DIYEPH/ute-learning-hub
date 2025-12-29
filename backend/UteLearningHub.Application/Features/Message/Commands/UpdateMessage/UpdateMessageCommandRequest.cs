namespace UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

public record UpdateMessageCommandRequest
{
    public Guid ConversationId { get; init; }
    public string Content { get; init; } = default!;
}
