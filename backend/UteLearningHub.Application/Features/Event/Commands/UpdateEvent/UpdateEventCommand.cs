using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Event.Commands.UpdateEvent;

public record UpdateEventCommand : UpdateEventRequest, IRequest<EventDto>
{
    public Guid Id { get; init; }
}