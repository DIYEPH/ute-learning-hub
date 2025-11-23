using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;

public record UpdateFacultyCommand : UpdateFacultyRequest, IRequest<FacultyDetailDto>;
