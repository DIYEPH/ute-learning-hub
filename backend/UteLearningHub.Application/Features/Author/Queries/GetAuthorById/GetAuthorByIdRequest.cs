namespace UteLearningHub.Application.Features.Author.Queries.GetAuthorById;

public record GetAuthorByIdRequest
{
    public Guid Id { get; init; }
}
