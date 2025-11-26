using MediatR;

namespace UteLearningHub.Application.Features.Event.Commands.DeleteEvent;

public record DeleteEventCommand(Guid Id) : IRequest<Unit>;