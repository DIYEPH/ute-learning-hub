namespace UteLearningHub.Application.Features.Message.Commands.UpdateMessage;

public record UpdateMessageRequest
{
    public Guid Id { get; init; }
    public string Content { get; init; } = default!;
}