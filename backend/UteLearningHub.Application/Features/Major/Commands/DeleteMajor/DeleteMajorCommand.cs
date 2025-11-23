using MediatR;

namespace UteLearningHub.Application.Features.Major.Commands.DeleteMajor;

public record DeleteMajorCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
