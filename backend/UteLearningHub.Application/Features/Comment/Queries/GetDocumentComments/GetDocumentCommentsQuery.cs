using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Queries.GetDocumentComments;

public record GetDocumentCommentsQuery : GetDocumentCommentsRequest, IRequest<PagedResponse<CommentDetailDto>>;


