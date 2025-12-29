using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

public record GetSubjectsQuery : GetSubjectsRequest, IRequest<PagedResponse<SubjectDetailDto>>;
