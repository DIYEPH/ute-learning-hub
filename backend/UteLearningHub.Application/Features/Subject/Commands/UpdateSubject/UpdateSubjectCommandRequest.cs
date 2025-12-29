namespace UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;

public record UpdateSubjectCommandRequest
{
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
    public List<Guid> MajorIds { get; init; } = [];
}
