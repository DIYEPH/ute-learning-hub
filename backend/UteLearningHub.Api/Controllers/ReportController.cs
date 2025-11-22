using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Report.Commands.CreateReport;
using UteLearningHub.Application.Features.Report.Commands.ReviewReport;
using UteLearningHub.Application.Features.Report.Queries.GetReports;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReportDto>> CreateReport([FromBody] CreateReportCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResponse<ReportDto>>> GetReports([FromQuery] GetReportsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/review")]
    [Authorize]
    public async Task<IActionResult> ReviewReport(Guid id, [FromBody] ReviewReportCommand command)
    {
        command = command with { ReportId = id };
        await _mediator.Send(command);
        return NoContent();
    }
}
