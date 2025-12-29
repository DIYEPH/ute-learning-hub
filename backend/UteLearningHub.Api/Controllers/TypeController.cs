using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Type.Commands.CreateType;
using UteLearningHub.Application.Features.Type.Commands.DeleteType;
using UteLearningHub.Application.Features.Type.Commands.UpdateType;
using UteLearningHub.Application.Features.Type.Queries.GetTypeById;
using UteLearningHub.Application.Features.Type.Queries.GetTypes;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TypeController : ControllerBase
{
    private readonly IMediator _mediator;

    public TypeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TypeDto>>> GetTypes([FromQuery] GetTypesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TypeDetailDto>> GetTypeById(Guid id)
    {
        var query = new GetTypeByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TypeDetailDto>> CreateType([FromBody] CreateTypeCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TypeDetailDto>> UpdateType(Guid id, [FromBody] UpdateTypeCommandRequest request)
    {
        var command = new UpdateTypeCommand
        {
            Id = id,
            TypeName = request.TypeName
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteType(Guid id)
    {
        var command = new DeleteTypeCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
