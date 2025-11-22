namespace UteLearningHub.Application.Features.Type.Queries.GetTypeById;

public record GetTypeByIdRequest
{
    public Guid Id { get; init; }
}