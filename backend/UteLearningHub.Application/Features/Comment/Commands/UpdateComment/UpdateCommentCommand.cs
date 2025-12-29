using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Commands.UpdateComment;

public record UpdateCommentCommand : UpdateCommentCommandRequest, IRequest<CommentDetailDto>
{
    public Guid Id { get; init; }
}
