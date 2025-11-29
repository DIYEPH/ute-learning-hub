namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public record CreateSubjectRequest
{
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
    public List<Guid> MajorIds { get; init; } = [];
}