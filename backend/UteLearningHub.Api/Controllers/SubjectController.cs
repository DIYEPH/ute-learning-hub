using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Subject.Commands.CreateSubject;
using UteLearningHub.Application.Features.Subject.Commands.DeleteSubject;
using UteLearningHub.Application.Features.Subject.Commands.UpdateSubject;
using UteLearningHub.Application.Features.Subject.Queries.GetSubjectById;
using UteLearningHub.Application.Features.Subject.Queries.GetSubjects;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubjectController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<SubjectDto>>> GetSubjects([FromQuery] GetSubjectsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubjectDetailDto>> GetSubjectById(Guid id)
    {
        var query = new GetSubjectByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDetailDto>> CreateSubject([FromBody] CreateSubjectCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubjectDetailDto>> UpdateSubject(Guid id, [FromBody] UpdateSubjectCommandRequest request)
    {
        var command = new UpdateSubjectCommand
        {
            Id = id,
            SubjectName = request.SubjectName,
            SubjectCode = request.SubjectCode,
            MajorIds = request.MajorIds
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSubject(Guid id)
    {
        var command = new DeleteSubjectCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
