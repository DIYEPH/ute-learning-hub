using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.ConversationJoinRequest.Commands.CreateConversationJoinRequest;
using UteLearningHub.Application.Features.ConversationJoinRequest.Commands.ReviewConversationJoinRequest;
using UteLearningHub.Application.Features.ConversationJoinRequest.Queries.GetConversationJoinRequests;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationJoinRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationJoinRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ConversationJoinRequestDto>> CreateJoinRequest([FromBody] CreateConversationJoinRequestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResponse<ConversationJoinRequestDto>>> GetJoinRequests([FromQuery] GetConversationJoinRequestsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewJoinRequest(Guid id, [FromBody] ReviewConversationJoinRequestCommand command)
    {
        command = command with { JoinRequestId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
