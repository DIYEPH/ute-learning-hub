namespace UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;

public record MarkMessageAsReadRequest
{
    public Guid MessageId { get; init; }
}