namespace UteLearningHub.Application.Features.Event.Commands.CreateEvent;

public record CreateEventRequest
{
    public string Title { get; init; } = default!;
    public string? ShortDescription { get; init; }
    public string? Content { get; init; }
    public string? ImageUrl { get; init; }
    public string? RedirectUrl { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public bool IsActive { get; init; } = true;
    public int Priority { get; init; } = 0;
}