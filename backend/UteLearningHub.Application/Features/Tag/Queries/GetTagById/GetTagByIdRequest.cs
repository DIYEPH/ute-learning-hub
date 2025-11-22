namespace UteLearningHub.Application.Features.Tag.Queries.GetTagById;

public record GetTagByIdRequest
{
    public Guid Id { get; init; }
}