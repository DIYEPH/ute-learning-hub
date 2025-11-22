using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Queries.GetComments;

public record GetCommentsRequest : PagedRequest
{
    public Guid DocumentId { get; init; }
    public Guid? ParentId { get; init; }
}
