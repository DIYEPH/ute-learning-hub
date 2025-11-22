using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
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
}
