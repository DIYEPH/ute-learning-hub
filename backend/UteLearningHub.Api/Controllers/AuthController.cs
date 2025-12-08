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
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login/microsoft")]
    public async Task<ActionResult<LoginWithMicrosoftResponse>> LoginWithMicrosoft([FromBody] LoginWithMicrosoftCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var command = new LogoutCommand();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        // Luôn trả về success để không leak thông tin về user tồn tại hay không
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
}
