using MediatR;

namespace UteLearningHub.Application.Features.Type.Commands.DeleteType;

public record DeleteTypeCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
