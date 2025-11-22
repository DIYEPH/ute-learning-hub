namespace UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;

public record GetSubjectByIdRequest
{
    public Guid Id { get; init; }
}