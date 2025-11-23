using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.User.Commands.UpdateUser;

public record UpdateUserCommand : UpdateUserRequest, IRequest<UserDto>
{
    public Guid UserId { get; init; }
}