namespace UteLearningHub.Application.Features.Message.Commands.PinMessage;

public record PinMessageRequest
{
    public Guid Id { get; init; }
    public bool IsPined { get; init; }
}