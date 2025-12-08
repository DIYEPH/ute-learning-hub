using MediatR;
using Microsoft.AspNetCore.Http;
using UteLearningHub.Application.Services.Authentication;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Domain.Exceptions;

namespace UteLearningHub.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogoutCommandHandler(
        IRefreshTokenService refreshTokenService,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _refreshTokenService = refreshTokenService;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
            throw new UnauthorizedException("You must be authenticated to logout");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedException();

        // Get sessionId from JWT token claim
        var sessionId = _httpContextAccessor.HttpContext?.User?.FindFirst("session_id")?.Value;

        if (string.IsNullOrEmpty(sessionId))
            return Unit.Value;

        // Revoke refresh token for this session
        await _refreshTokenService.RevokeRefreshTokenAsync(userId, sessionId, cancellationToken);

        return Unit.Value;
    }
}
