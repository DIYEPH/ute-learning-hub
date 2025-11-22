using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Major.Queries.GetMajorById;
using UteLearningHub.Application.Features.Major.Queries.GetMajors;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MajorController : ControllerBase
{
    private readonly IMediator _mediator;

    public MajorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<MajorDto>>> GetMajors([FromQuery] GetMajorsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MajorDetailDto>> GetMajorById(Guid id)
    {
        var query = new GetMajorByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
