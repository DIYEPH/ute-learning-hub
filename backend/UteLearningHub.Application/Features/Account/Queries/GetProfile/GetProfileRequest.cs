namespace UteLearningHub.Application.Features.Account.Queries.GetProfile;

public record GetProfileRequest
{
    public Guid? UserId { get; init; }
}
