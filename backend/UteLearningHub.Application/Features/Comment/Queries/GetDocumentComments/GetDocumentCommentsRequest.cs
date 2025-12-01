using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Queries.GetDocumentComments;

public record GetDocumentCommentsRequest : PagedRequest
{
    public Guid DocumentId { get; init; }
    public Guid? ParentId { get; init; }
}


