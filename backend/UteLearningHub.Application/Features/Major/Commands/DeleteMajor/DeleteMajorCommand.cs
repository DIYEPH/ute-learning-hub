using MediatR;

namespace UteLearningHub.Application.Features.Major.Commands.DeleteMajor;

public record DeleteMajorCommand : IRequest
{
    public Guid Id { get; init; }
}
