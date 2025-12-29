using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public record CreateSubjectCommand : IRequest<SubjectDetailDto>
{
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
    public List<Guid> MajorIds { get; init; } = [];
}
