using MediatR;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityService _identityService;
    public LoginCommandHandler(IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, IIdentityService identityService)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _identityService = identityService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by email or username
        var user = (request.EmailOrUsername.Contains('@')
            ? await _identityService.FindByEmailAsync(request.EmailOrUsername)
            : await _identityService.FindByUsernameAsync(request.EmailOrUsername));

        if (user == null)
            throw new UnauthorizedException("Email hoặc mật khẩu không chính xác.");

        // 2. Check if account is locked
        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            var lockoutEnd = user.LockoutEnd.Value.ToLocalTime();
            var isPermanent = lockoutEnd.Year >= DateTimeOffset.UtcNow.Year + 50;
            
            if (isPermanent)
                throw new UnauthorizedException("Tài khoản của bạn đã bị khóa vĩnh viễn. Vui lòng liên hệ quản trị viên.");
            else
                throw new UnauthorizedException($"Tài khoản của bạn đã bị khóa đến {lockoutEnd:dd/MM/yyyy HH:mm}.");
        }

        // 3. Check password
        var isPasswordValid = await _identityService.CheckPasswordAsync(user.Id, request.Password);
        if (!isPasswordValid)
            throw new UnauthorizedException("Email hoặc mật khẩu không chính xác.");

        // 4. Generate tokens
        var sessionId = Guid.NewGuid().ToString();
        var roles = await _identityService.GetRolesAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.UserName, roles, sessionId);
        var refreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, sessionId);

        // 5. Update last login
        await _identityService.UpdateLastLoginAsync(user.Id, cancellationToken);

        return new LoginResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email!,
            FullName = user.FullName,
            Username = user.UserName,
            AvatarUrl = user.AvatarUrl,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
