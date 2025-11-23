namespace UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;

public record CreateFacultyRequest
{
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
}