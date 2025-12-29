using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Major.Commands.CreateMajor;
using UteLearningHub.Application.Features.Major.Commands.DeleteMajor;
using UteLearningHub.Application.Features.Major.Commands.UpdateMajor;
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
    public async Task<ActionResult<PagedResponse<MajorDetailDto>>> GetMajors([FromQuery] GetMajorsQuery query)
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MajorDetailDto>> CreateMajor([FromBody] CreateMajorCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MajorDetailDto>> UpdateMajor(Guid id, [FromBody] UpdateMajorCommandRequest request)
    {
        var command = new UpdateMajorCommand
        {
            Id = id,
            FacultyId = request.FacultyId,
            MajorName = request.MajorName,
            MajorCode = request.MajorCode
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMajor(Guid id)
    {
        var command = new DeleteMajorCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
