namespace UteLearningHub.Application.Features.Comment.Commands.CreateComment;

public record CreateCommentRequest
{
    public Guid DocumentFileId { get; init; }
    public Guid? ParentId { get; init; }
    public string Content { get; init; } = default!;
}
