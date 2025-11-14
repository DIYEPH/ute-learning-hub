using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Auth.Commands.Login;
using UteLearningHub.Application.Features.Auth.Commands.LoginWithMicrosoft;
using UteLearningHub.Application.Features.Auth.Commands.RefreshToken;

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

    /// <summary>
    /// Login with email/username and password
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Login with Microsoft account
    /// </summary>
    [HttpPost("login/microsoft")]
    public async Task<ActionResult<LoginWithMicrosoftResponse>> LoginWithMicrosoft([FromBody] LoginWithMicrosoftCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

}
