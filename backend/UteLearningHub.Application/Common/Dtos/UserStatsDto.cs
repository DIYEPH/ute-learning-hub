namespace UteLearningHub.Application.Common.Dtos;

public record UserStatsDto
{
    public int Uploads { get; init; }
    public int Upvotes { get; init; }
    public int Comments { get; init; }
}
