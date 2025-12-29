using MediatR;

namespace UteLearningHub.Application.Features.Subject.Commands.DeleteSubject;

public record DeleteSubjectCommand : IRequest
{
    public Guid Id { get; init; }
}
