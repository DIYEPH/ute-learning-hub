using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.User.Commands.BanUser;
using UteLearningHub.Application.Features.User.Commands.ManageTrustScore;
using UteLearningHub.Application.Features.User.Commands.UnbanUser;
using UteLearningHub.Application.Features.User.Commands.UpdateUser;
using UteLearningHub.Application.Features.User.Queries.GetUserById;
using UteLearningHub.Application.Features.User.Queries.GetUsers;
using UteLearningHub.Application.Features.User.Queries.GetUserTrustHistory;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetUsers([FromQuery] GetUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        command = command with { UserId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/ban")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BanUser(Guid id, [FromBody] BanUserCommand command)
    {
        command = command with { UserId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/unban")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnbanUser(Guid id)
    {
        var command = new UnbanUserCommand { UserId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id}/trust-score")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> ManageTrustScore(Guid id, [FromBody] ManageTrustScoreCommand command)
    {
        command = command with { UserId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{id}/trust-history")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IList<UserTrustHistoryDto>>> GetUserTrustHistory(Guid id)
    {
        var query = new GetUserTrustHistoryQuery { UserId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
