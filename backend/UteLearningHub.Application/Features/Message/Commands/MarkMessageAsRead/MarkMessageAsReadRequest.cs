namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public record MarkMessageAsReadRequest
{
    public Guid ConversationId { get; init; }
    public Guid MessageId { get; init; }
}