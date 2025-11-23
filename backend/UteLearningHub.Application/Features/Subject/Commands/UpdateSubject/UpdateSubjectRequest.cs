namespace UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;

public record UpdateSubjectRequest
{
    public Guid Id { get; init; }
    public Guid MajorId { get; init; }
    public string SubjectName { get; init; } = default!;
    public string SubjectCode { get; init; } = default!;
}