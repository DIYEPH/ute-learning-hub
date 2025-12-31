using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Account.Queries.GetUserStats;

public record GetUserStatsQuery : IRequest<UserStatsDto>
{
    public Guid? UserId { get; init; }
}
