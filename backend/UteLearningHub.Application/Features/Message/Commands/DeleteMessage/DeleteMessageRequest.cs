namespace UteLearningHub.Application.Features.Message.Commands.DeleteMessage;

public record DeleteMessageRequest
{
    public Guid Id { get; init; }
}