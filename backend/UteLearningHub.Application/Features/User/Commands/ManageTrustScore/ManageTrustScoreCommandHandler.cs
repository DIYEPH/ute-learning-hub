using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.User.Commands.ManageTrustScore;

public class ManageTrustScoreCommandHandler : IRequestHandler<ManageTrustScoreCommand, UserDto>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public ManageTrustScoreCommandHandler(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(ManageTrustScoreCommand request, CancellationToken cancellationToken)
    {
        // Only admin can manage trust score
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to manage trust score");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can manage trust score");

        return await _userService.UpdateTrustScoreAsync(request.UserId, request.TrustScore, request.Reason, null, TrustEntityType.Manual, cancellationToken);
    }
}
