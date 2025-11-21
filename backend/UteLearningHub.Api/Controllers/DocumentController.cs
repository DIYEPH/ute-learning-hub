using MediatR;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Queries.GetDocuments;

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
    }
}
