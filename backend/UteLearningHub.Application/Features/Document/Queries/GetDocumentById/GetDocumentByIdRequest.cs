namespace UteLearningHub.Application.Features.Document.Queries.GetDocumentById;

public record GetDocumentByIdRequest
{
    public Guid Id { get; init; }
}