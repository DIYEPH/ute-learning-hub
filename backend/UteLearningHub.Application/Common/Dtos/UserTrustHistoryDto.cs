namespace UteLearningHub.Application.Common.Dtos;

public record UserTrustHistoryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public double Score { get; init; }
    public double OldScore { get; init; }
    public double NewScore { get; init; }
    public string Reason { get; init; } = default!;
    public Guid? EntityId { get; init; }
    public int? EntityType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
