using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Event.Commands.CreateEvent;

public record CreateEventCommand : CreateEventRequest, IRequest<EventDto>;