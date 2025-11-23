using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.ConversationJoinRequest.Commands.CreateConversationJoinRequest;

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
}
