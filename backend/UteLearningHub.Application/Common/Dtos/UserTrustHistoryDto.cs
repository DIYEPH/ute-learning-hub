namespace UteLearningHub.Application.Common.Dtos;

public record UserTrustHistoryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public double Score { get; init; }
    public double OldScore { get; init; }
    public double NewScore { get; init; }
    public string Reason { get; init; } = default!;
    public DateTimeOffset CreatedAt { get; init; }
}
