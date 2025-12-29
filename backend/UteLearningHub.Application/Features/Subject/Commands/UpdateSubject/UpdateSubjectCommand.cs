using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;

public record UpdateSubjectCommand : UpdateSubjectCommandRequest, IRequest<SubjectDetailDto>
{
    public Guid Id { get; init; }
}
