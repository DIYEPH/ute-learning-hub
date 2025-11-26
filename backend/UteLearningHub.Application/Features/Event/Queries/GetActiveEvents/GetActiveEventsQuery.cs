using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Event.Queries.GetActiveEvents;

public record GetActiveEventsQuery
    : GetActiveEventsRequest, IRequest<IReadOnlyList<EventDto>>;