using MediatR;

namespace UteLearningHub.Application.Features.Faculty.Commands.DeleteFaculty;

public record DeleteFacultyCommand : IRequest
{
    public Guid Id { get; init; }
}
