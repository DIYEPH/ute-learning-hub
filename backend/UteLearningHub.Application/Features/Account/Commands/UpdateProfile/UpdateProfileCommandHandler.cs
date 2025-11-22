using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Account.Commands.UpdateProfile;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public UpdateProfileCommandHandler(
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _currentUserService = currentUserService;
        _userService = userService;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to update your profile");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        return await _userService.UpdateProfileAsync(userId, request, cancellationToken);
    }
}