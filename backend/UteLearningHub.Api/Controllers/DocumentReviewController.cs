using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Document.Commands.CreateDocumentReview;

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
    public async Task<ActionResult> CreateDocumentReview([FromBody] CreateOrUpdateDocumentFileReviewCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }
}
