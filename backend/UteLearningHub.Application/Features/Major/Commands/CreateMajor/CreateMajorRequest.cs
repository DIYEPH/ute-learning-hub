namespace UteLearningHub.Application.Features.Major.Commands.CreateMajor;

public record CreateMajorRequest
{
    public Guid FacultyId { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
}