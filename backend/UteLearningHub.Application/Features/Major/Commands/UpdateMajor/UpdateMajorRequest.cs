namespace UteLearningHub.Application.Features.Major.Commands.UpdateMajor;

public record UpdateMajorRequest
{
    public Guid Id { get; init; }
    public Guid FacultyId { get; init; }
    public string MajorName { get; init; } = default!;
    public string MajorCode { get; init; } = default!;
}
