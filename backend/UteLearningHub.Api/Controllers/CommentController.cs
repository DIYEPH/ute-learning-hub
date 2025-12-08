using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Comment.Commands.CreateComment;
using UteLearningHub.Application.Features.Comment.Commands.DeleteComment;
using UteLearningHub.Application.Features.Comment.Commands.UpdateComment;
using UteLearningHub.Application.Features.Comment.Queries.GetComments;
using UteLearningHub.Application.Features.Comment.Queries.GetDocumentComments;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<CommentDto>>> GetComments([FromQuery] GetCommentsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("document")]
    public async Task<ActionResult<PagedResponse<CommentDto>>> GetDocumentComments([FromQuery] GetDocumentCommentsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> UpdateComment(Guid id, [FromBody] UpdateCommentCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var command = new DeleteCommentCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
