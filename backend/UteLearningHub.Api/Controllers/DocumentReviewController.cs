using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.DocumentReview.Commands.CreateDocumentReview;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DocumentReviewController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentReviewController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DocumentReviewDto>> CreateDocumentReview([FromBody] CreateDocumentReviewCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
