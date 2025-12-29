using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.Account.Queries.GetProfile;

public record GetProfileQuery : IRequest<ProfileDetailDto>
{
    public Guid? UserId { get; init; }
}
