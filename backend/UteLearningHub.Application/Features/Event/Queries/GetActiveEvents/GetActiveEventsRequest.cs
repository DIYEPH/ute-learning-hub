namespace UteLearningHub.Application.Features.Event.Queries.GetActiveEvents;

public record GetActiveEventsRequest
{
    public int? Take { get; init; } = 10;
}