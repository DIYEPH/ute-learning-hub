using MediatR;
using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Features.User.Queries.GetUsers;

public record GetUsersQuery : GetUsersRequest, IRequest<PagedResponse<UserDto>>;