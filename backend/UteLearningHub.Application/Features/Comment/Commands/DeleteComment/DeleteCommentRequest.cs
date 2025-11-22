namespace UteLearningHub.Application.Features.Comment.Commands.DeleteComment;

public record DeleteCommentRequest
{
    public Guid Id { get; init; }
}