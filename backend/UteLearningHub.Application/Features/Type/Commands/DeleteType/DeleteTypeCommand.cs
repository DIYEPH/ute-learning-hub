using MediatR;

namespace UteLearningHub.Application.Features.Type.Commands.DeleteType;

public record DeleteTypeCommand : IRequest
{
    public Guid Id { get; init; }
}
