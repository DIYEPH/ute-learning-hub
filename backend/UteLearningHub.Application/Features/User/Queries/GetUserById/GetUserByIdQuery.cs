using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.User.Queries.GetUserById;

public record GetUserByIdQuery : IRequest<UserDto>
{
    public Guid UserId { get; init; }
}
