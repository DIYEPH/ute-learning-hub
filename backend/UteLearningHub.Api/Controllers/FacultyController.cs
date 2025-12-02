using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Faculty.Commands.CreateFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.DeleteFaculty;
using UteLearningHub.Application.Features.Faculty.Commands.UpdateFaculty;
using UteLearningHub.Application.Features.Faculty.Queries.GetFacultyById;
using UteLearningHub.Application.Features.Faculty.Queries.GetFaculties;
using UteLearningHub.Application.Services.FileStorage;

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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacultyDetailDto>> CreateFaculty([FromBody] CreateFacultyCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FacultyDetailDto>> UpdateFaculty(Guid id, [FromBody] UpdateFacultyCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFaculty(Guid id)
    {
        var command = new DeleteFacultyCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    // Upload-logo endpoint removed: use /api/File + Create/UpdateFaculty with logo URL instead
}
