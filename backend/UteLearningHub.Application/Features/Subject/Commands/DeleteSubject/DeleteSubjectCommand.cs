using MediatR;

namespace UteLearningHub.Application.Features.Subject.Commands.DeleteSubject;

public record DeleteSubjectCommand : IRequest<Unit>
{
    public Guid Id { get; init; }
}
