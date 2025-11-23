using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;

public record CreateFacultyCommand : CreateFacultyRequest, IRequest<FacultyDetailDto>;