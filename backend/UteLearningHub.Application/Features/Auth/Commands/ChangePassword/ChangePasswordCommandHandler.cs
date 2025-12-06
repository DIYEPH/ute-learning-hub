using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to change password");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        var (succeeded, errors) = await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!succeeded)
        {
            var errorMessage = string.Join(", ", errors);
            throw new BadRequestException($"Không thể đổi mật khẩu: {errorMessage}");
        }

        return Unit.Value;
    }
}


