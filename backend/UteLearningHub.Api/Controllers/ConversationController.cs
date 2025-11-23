using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;
using UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;
using UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversationById;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversations;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResponse<ConversationDto>>> GetConversations([FromQuery] GetConversationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> GetConversationById(Guid id)
    {
        var query = new GetConversationByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> CreateConversation([FromBody] CreateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        var command = new DeleteConversationCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
