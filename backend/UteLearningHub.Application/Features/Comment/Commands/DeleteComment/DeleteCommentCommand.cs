using MediatR;

namespace UteLearningHub.Application.Features.Comment.Commands.DeleteComment;

public record DeleteCommentCommand : IRequest
{
    public Guid Id { get; init; }
}
