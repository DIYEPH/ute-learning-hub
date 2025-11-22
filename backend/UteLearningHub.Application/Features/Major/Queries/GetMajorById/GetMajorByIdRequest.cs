namespace UteLearningHub.Application.Features.Major.Queries.GetMajorById;

public record GetMajorByIdRequest
{
    public Guid Id { get; init; }
}