using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Queries.GetComments;

public record GetCommentsQuery : GetCommentsRequest, IRequest<PagedResponse<CommentDetailDto>>;
