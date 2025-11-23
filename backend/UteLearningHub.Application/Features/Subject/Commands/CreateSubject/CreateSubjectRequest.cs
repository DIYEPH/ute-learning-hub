namespace UteLearningHub.Application.Features.Subject.Commands.CreateSubject;

public record CreateSubjectRequest
{
    public Guid MajorId { get; init; }
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
}