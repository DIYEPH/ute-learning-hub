using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Tag.Commands.CreateTag;
using UteLearningHub.Application.Features.Tag.Commands.DeleteTag;
using UteLearningHub.Application.Features.Tag.Commands.UpdateTag;
using UteLearningHub.Application.Features.Tag.Queries.GetTagById;
using UteLearningHub.Application.Features.Tag.Queries.GetTags;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TagDto>>> GetTags([FromQuery] GetTagsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TagDetailDto>> GetTagById(Guid id)
    {
        var query = new GetTagByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TagDetailDto>> CreateTag([FromBody] CreateTagCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<TagDetailDto>> UpdateTag(Guid id, [FromBody] UpdateTagCommandRequest request)
    {
        var command = new UpdateTagCommand
        {
            Id = id,
            TagName = request.TagName
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var command = new DeleteTagCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
