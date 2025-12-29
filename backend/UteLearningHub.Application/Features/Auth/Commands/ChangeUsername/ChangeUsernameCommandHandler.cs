using MediatR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.ChangeUsername;

public class ChangeUsernameCommandHandler : IRequestHandler<ChangeUsernameCommand, Unit>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public ChangeUsernameCommandHandler(
        IIdentityService identityService,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(ChangeUsernameCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("Bạn cần đăng nhập để đổi username.");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();
        var user = await _identityService.FindByIdAsync(userId)
            ?? throw new NotFoundException("User không tồn tại.");

        if (string.IsNullOrWhiteSpace(request.NewUsername))
            throw new BadRequestException("Username mới không hợp lệ.");

        var normalizedNewUsername = request.NewUsername.Trim();

        // Check if username is same as current
        if (string.Equals(user.UserName, normalizedNewUsername, StringComparison.OrdinalIgnoreCase))
            return Unit.Value;

        // Username cannot be email format (except user's own email)
        if (normalizedNewUsername.Contains('@'))
        {
            if (!string.Equals(normalizedNewUsername, user.Email, StringComparison.OrdinalIgnoreCase))
                throw new BadRequestException("Username không được có định dạng email.");
        }

        var existingUser = await _identityService.FindByUsernameAsync(normalizedNewUsername);
        if (existingUser != null && existingUser.Id != user.Id)
            throw new BadRequestException("Username đã được sử dụng. Vui lòng chọn username khác.");

        var (succeeded, errors) = await _identityService.UpdateUsernameAsync(userId, normalizedNewUsername);
        if (!succeeded)
            throw new BadRequestException($"Không thể đổi username: {string.Join(", ", errors)}");

        return Unit.Value;
    }
}

