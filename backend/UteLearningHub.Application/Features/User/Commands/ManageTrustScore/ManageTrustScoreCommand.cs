using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.User.Commands.ManageTrustScore;

public record ManageTrustScoreCommand : ManageTrustScoreRequest, IRequest<UserDto>
{
    public Guid UserId { get; init; }
}
