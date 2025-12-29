namespace UteLearningHub.Application.Features.Comment.Commands.UpdateComment;

public record UpdateCommentCommandRequest
{
    public string Content { get; init; } = default!;
}
