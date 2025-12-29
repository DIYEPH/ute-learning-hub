using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Auth.Commands.ChangePassword;
using UteLearningHub.Application.Features.Auth.Commands.ChangeUsername;
using UteLearningHub.Application.Features.Auth.Commands.ForgotPassword;
using UteLearningHub.Application.Features.Auth.Commands.Login;
using UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;
using UteLearningHub.Application.Features.Auth.Commands.Logout;
using UteLearningHub.Application.Features.Auth.Commands.RefreshToken;
using UteLearningHub.Application.Features.Auth.Commands.ResetPassword;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("login/microsoft")]
    public async Task<ActionResult<LoginWithMicrosoftResponse>> LoginWithMicrosoft([FromBody] LoginWithMicrosoftCommand command)
    {
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken()
    {
        // Read refresh token from httpOnly cookie
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token not found" });
        }

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var result = await _mediator.Send(command);

        // Set new refresh token in cookie
        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        await _mediator.Send(command);

        // Clear refresh token cookie
        ClearRefreshTokenCookie();
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Nếu email tồn tại, bạn sẽ nhận được email hướng dẫn đặt lại mật khẩu." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { message = "Mật khẩu đã được đặt lại thành công." });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("change-username")]
    [Authorize]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsernameCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    // ============ Cookie Helpers ============

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var expiryDays = _configuration.GetValue<int>("Jwt:RefreshTokenExpiryDays", 7);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
            Path = "/"
        };
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        });
    }
}