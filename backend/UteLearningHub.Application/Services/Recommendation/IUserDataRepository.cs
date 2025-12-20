namespace UteLearningHub.Application.Services.Recommendation;

public interface IUserDataRepository
{
    Task<UserBehaviorTextDataDto?> GetUserBehaviorTextDataAsync(Guid userId, CancellationToken cancellationToken = default);
}

public record UserBehaviorTextDataDto(
    Guid UserId,
    IReadOnlyList<TextScoreItem> SubjectScores,
    IReadOnlyList<TextScoreItem> TagScores
);

public record TextScoreItem(string Name, int Score);
