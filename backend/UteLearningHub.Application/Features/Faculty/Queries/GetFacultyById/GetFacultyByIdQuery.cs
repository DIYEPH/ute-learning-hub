using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;

public record GetFacultyByIdQuery : GetFacultyByIdRequest, IRequest<FacultyDetailDto>;