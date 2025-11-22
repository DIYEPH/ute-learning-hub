namespace UteLearningHub.Application.Services.Comment;

public interface ICommentService
{
    Task<Dictionary<Guid, CommentAuthorInfo>> GetCommentAuthorsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
    Task<int> GetReplyCountAsync(Guid commentId, CancellationToken cancellationToken = default);
}

public record CommentAuthorInfo
{
    public string FullName { get; init; } = default!;
    public string? AvatarUrl { get; init; }
}