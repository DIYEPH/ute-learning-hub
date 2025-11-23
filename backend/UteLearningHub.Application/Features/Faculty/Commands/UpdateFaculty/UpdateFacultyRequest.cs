namespace UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;

public record UpdateFacultyRequest
{
    public Guid Id { get; init; }
    public string FacultyName { get; init; } = default!;
    public string FacultyCode { get; init; } = default!;
}
