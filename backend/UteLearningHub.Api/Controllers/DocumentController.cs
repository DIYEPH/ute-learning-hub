using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Features.Document.Queries.GetDocumentById;
using UteLearningHub.Application.Features.Document.Queries.GetDocuments;
using UteLearningHub.Application.Features.Document.Commands.ReviewDocument;

namespace UteLearningHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DocumentController(IMediator mediator) { 
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<DocumentDto>>> GetDocuments([FromQuery] GetDocumentsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDetailDto>> GetDocumentById(Guid id)
        {
            var query = new GetDocumentByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<DocumentDetailDto>> UpdateDocument(Guid id, [FromBody] UpdateDocumentCommand command)
        {
            command = command with { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Unit>> DeleteSoftDocumentById(Guid id)
        {
            var command = new DeleteDocumentsCommand { Id = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/review")]
        [Authorize]
        public async Task<IActionResult> ReviewDocument(Guid id, [FromBody] ReviewDocumentCommand command)
        {
            command = command with { DocumentId = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
