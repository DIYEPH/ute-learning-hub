using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;

public record CreateFacultyCommand : IRequest<FacultyDetailDto>
{
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
    public string? Logo { get; init; }
}