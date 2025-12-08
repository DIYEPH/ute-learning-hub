using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Message.Commands.CreateMessage;
using UteLearningHub.Application.Features.Message.Commands.DeleteMessage;
using UteLearningHub.Application.Features.Message.Commands.MarkMessageAsRead;
using UteLearningHub.Application.Features.Message.Commands.PinMessage;
using UteLearningHub.Application.Features.Message.Commands.UpdateMessage;
using UteLearningHub.Application.Features.Message.Queries.GetMessages;

namespace UteLearningHub.Api.Controllers;

[Route("api/conversations/{conversationId}/messages")]
[ApiController]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessageController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<MessageDto>>> GetMessages(
        Guid conversationId,
        [FromQuery] GetMessagesQuery query)
    {
        query = query with { ConversationId = conversationId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(
        Guid conversationId,
        [FromBody] CreateMessageCommand command)
    {
        command = command with { ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MessageDto>> UpdateMessage(
        Guid conversationId,
        Guid id,
        [FromBody] UpdateMessageCommand command)
    {
        command = command with { Id = id, ConversationId = conversationId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(Guid conversationId, Guid id)
    {
        var command = new DeleteMessageCommand { Id = id, ConversationId = conversationId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/pin")]
    public async Task<IActionResult> PinMessage(
        Guid conversationId,
        Guid id,
        [FromBody] PinMessageCommand command)
    {
        command = command with { Id = id, ConversationId = conversationId };
        await _mediator.Send(command);
        return NoContent();
    }
    [HttpPost("{id}/mark-as-read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid conversationId, Guid id)
    {
        var command = new MarkMessageAsReadCommand { MessageId = id, ConversationId = conversationId };
        await _mediator.Send(command);
        return NoContent();
    }
}