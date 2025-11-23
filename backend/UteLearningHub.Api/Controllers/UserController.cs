using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.User.Queries.GetUsers;

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
}
