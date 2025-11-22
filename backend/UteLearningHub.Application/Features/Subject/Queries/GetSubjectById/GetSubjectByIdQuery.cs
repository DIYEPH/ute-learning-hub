using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;

public record GetSubjectByIdQuery : GetSubjectByIdRequest, IRequest<SubjectDetailDto>;
