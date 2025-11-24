namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public record PinMessageRequest
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public bool IsPined { get; init; }
}