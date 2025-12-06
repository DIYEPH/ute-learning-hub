namespace UteLearningHub.Application.Services.Recommendation;

public interface IUserDataRepository
{
    Task<UserBehaviorDataDto?> GetUserBehaviorDataAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregated user behavior data for vector calculation
/// Scores are calculated from: Documents created, Conversations joined, Useful/NotUseful votes
/// </summary>
public record UserBehaviorDataDto(
    Guid UserId,
    /// <summary>
    /// Faculty scores aggregated from Documents (with Subject) and Conversations (with Subject)
    /// Score weights: Document created = +3, Conversation joined = +2, Useful vote = +1, NotUseful vote = -1
    /// </summary>
    IReadOnlyList<ScoreItem> FacultyScores,
    /// <summary>
    /// Type (loại tài liệu) scores from Documents
    /// Score weights: Document created = +3, Useful vote = +1, NotUseful vote = -1
    /// </summary>
    IReadOnlyList<ScoreItem> TypeScores,
    /// <summary>
    /// Tag scores from Documents and Conversations
    /// Score weights: Document created = +3/tag, Conversation joined = +2/tag, Useful = +1/tag, NotUseful = -1/tag
    /// </summary>
    IReadOnlyList<ScoreItem> TagScores
);

public record ScoreItem(Guid Id, int Score);

// Keep old DTO for backwards compatibility (will be removed later)
[Obsolete("Use UserBehaviorDataDto instead")]
public record UserDataDto(
    Guid UserId,
    Guid? MajorId,
    IReadOnlyList<Guid> SubjectIds,
    IReadOnlyList<Guid> TagIds
);
