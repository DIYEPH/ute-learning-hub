using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.User;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.User.Commands.UnbanUser;

public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand, Unit>
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UnbanUserCommandHandler(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        // Only admin can unban users
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to unban users");

        var isAdmin = _currentUserService.IsInRole("Admin");
        if (!isAdmin)
            throw new UnauthorizedException("Only administrators can unban users");

        await _userService.UnbanUserAsync(request.UserId, cancellationToken);
        return Unit.Value;
    }
}
