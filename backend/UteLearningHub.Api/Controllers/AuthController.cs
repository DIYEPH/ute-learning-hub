using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Auth.Commands.Login;
using UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;
using UteLearningHub.Application.Features.Auth.Commands.RefreshToken;
using UteLearningHub.Application.Features.Auth.Commands.Logout;
using UteLearningHub.Application.Features.Auth.Commands.CompleteAccountSetup;

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

    [HttpPost("setup")]
    [Authorize]
    public async Task<ActionResult<CompleteAccountSetupResponse>> CompleteSetup(
        [FromBody] CompleteAccountSetupCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
