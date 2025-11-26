namespace UteLearningHub.Application.Features.Event.Commands.UpdateEvent;

public record UpdateEventRequest
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? ShortDescription { get; init; }
    public string? Content { get; init; }
    public string? ImageUrl { get; init; }
    public string? RedirectUrl { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public bool IsActive { get; init; }
    public int Priority { get; init; }
}