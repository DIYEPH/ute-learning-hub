namespace UteLearningHub.Application.Features.User.Commands.ManageTrustScore;

public record ManageTrustScoreRequest
{
    public int TrustScore { get; init; }
    public string? Reason { get; init; } // Lý do thay đổi trust score
}