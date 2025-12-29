using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Comment.Commands.CreateComment;

public record CreateCommentCommand : IRequest<CommentDetailDto>
{
    public Guid DocumentFileId { get; init; }
    public Guid? ParentId { get; init; }
    public string Content { get; init; } = default!;
}
