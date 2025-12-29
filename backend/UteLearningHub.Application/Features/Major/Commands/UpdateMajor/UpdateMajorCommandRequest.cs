namespace UteLearningHub.Application.Features.Major.Commands.UpdateMajor;

public record UpdateMajorCommandRequest
{
    public Guid FacultyId { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
}
