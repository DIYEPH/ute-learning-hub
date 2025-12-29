using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

public record GetFacultiesQuery : GetFacultiesRequest, IRequest<PagedResponse<FacultyDetailDto>>;