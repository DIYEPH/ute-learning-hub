using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.User.Queries.GetUserById;

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByIdHandler(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // Only admin can view user details
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to view user details");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can view user details");

        var user = await _userService.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException($"User with id {request.UserId} not found");

        return user;
    }
}