using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.User.Queries.GetUserTrustHistory;

public record GetUserTrustHistoryQuery : IRequest<IList<UserTrustHistoryDto>>
{
    public Guid UserId { get; init; }
}
