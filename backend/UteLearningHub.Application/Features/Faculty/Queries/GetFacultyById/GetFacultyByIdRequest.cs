namespace UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;

public record GetFacultyByIdRequest
{
    public Guid Id { get; init; }
}