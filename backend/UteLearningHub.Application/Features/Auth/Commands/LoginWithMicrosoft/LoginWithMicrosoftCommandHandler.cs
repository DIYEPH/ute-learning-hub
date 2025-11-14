using MediatR;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;

public class LoginWithMicrosoftCommandHandler : IRequestHandler<LoginWithMicrosoftCommand, LoginWithMicrosoftResponse>
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IMicrosoftTokenValidator _microsoftTokenValidator;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityService _identityService;
    public LoginWithMicrosoftCommandHandler(IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, IIdentityService identityService, IMicrosoftTokenValidator microsoftTokenValidator)
    {
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _identityService = identityService;
        _microsoftTokenValidator = microsoftTokenValidator;
    }

    public async Task<LoginWithMicrosoftResponse> Handle(LoginWithMicrosoftCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Microsoft token
        var microsoftUser = await _microsoftTokenValidator.ValidateTokenAsync(request.IdToken, cancellationToken);
        if (microsoftUser == null)
            throw new UnauthorizedAccessException("Invalid Microsoft token");

        const string loginProvider = "Microsoft";
        var sessionId = Guid.NewGuid().ToString();

        // 2. Find user by external login
        var user = await _identityService.FindByExternalLoginAsync(
           loginProvider,
           microsoftUser.MicrosoftUserId
        );

        if (user == null)
        {
            // 3. Check if email exists
            user = await _identityService.FindByEmailAsync(microsoftUser.Email);
            if (user != null)
            {
                // Link external login to existing user
                var linkSuccess = await _identityService.AddExternalLoginAsync(
                    user.Id,
                    new ExternalLoginInfoDto(
                        loginProvider,
                        microsoftUser.MicrosoftUserId,
                        "Microsoft Account"
                    )
                );
                if (!linkSuccess)
                    throw new Exception("Failed to link Microsoft account");
            }
            else
            {
                // 4. Create new user
                var (succeeded, userId, errors) = await _identityService.CreateUserAsync(
                    new CreateUserDto(
                        microsoftUser.Email,
                        null,
                        microsoftUser.Name,
                        true,
                        null,
                        null,
                        null
                        ));

                if (!succeeded)
                    throw new Exception($"Failed to create user: {string.Join(", ", errors)}");

                await _identityService.AddExternalLoginAsync(
                    userId,
                    new ExternalLoginInfoDto(
                        loginProvider,
                        microsoftUser.MicrosoftUserId,
                        "Microsoft Account"
                    )
                );
                await _identityService.AddToRoleAsync(userId, "User");
                user = await _identityService.FindByIdAsync(userId);
            }
        }
        if (user == null)
            throw new Exception("User not found after creation");
        // 5. Generate tokens
        var roles = await _identityService.GetRolesAsync(user.Id);
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.UserName, roles, sessionId);
        var refreshToken = await _refreshTokenService.GenerateAndSaveRefreshTokenAsync(user.Id, sessionId);
        return new LoginWithMicrosoftResponse
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
