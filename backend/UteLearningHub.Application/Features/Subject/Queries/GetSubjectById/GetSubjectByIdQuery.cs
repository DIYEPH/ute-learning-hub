using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;

public record GetSubjectByIdQuery : IRequest<SubjectDetailDto>
{
    public Guid Id { get; init; }
}
