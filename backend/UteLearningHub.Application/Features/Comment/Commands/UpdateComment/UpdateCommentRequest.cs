namespace UteLearningHub.Application.Features.Comment.Commands.UpdateComment;

public record UpdateCommentRequest
{
    public Guid Id { get; init; }
    public string Content { get; init; } = default!;
}