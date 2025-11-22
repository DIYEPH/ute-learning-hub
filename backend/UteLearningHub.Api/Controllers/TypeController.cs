using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
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
}
