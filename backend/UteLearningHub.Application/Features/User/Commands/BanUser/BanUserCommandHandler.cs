using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.User.Commands.BanUser;

public class BanUserCommandHandler : IRequestHandler<BanUserCommand, Unit>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public BanUserCommandHandler(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        // Only admin can ban users
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to ban users");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can ban users");

        await _userService.BanUserAsync(request.UserId, request.BanUntil, cancellationToken);
        return Unit.Value;
    }
}