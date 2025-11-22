using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;
using UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FacultyController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacultyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<FacultyDto>>> GetFaculties([FromQuery] GetFacultiesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FacultyDetailDto>> GetFacultyById(Guid id)
    {
        var query = new GetFacultyByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
