using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Document.Commands.AddDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.CreateDocument;
using UteLearningHub.Application.Features.Document.Commands.DeleteDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.DeleteDocuments;
using UteLearningHub.Application.Features.Document.Commands.IncrementViewCount;
using UteLearningHub.Application.Features.Document.Commands.ResubmitDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.ReviewDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocument;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocumentFile;
using UteLearningHub.Application.Features.Document.Commands.UpdateDocumentProgress;
using UteLearningHub.Application.Features.Document.Queries.GetDocumentById;
using UteLearningHub.Application.Features.Document.Queries.GetDocumentProgress;
using UteLearningHub.Application.Features.Document.Queries.GetDocuments;
using UteLearningHub.Application.Features.Document.Queries.GetHomepage;
using UteLearningHub.Application.Features.Document.Queries.GetMyDocuments;
using UteLearningHub.Application.Features.Document.Queries.GetPendingFilesCount;
using UteLearningHub.Application.Features.Document.Queries.GetReadingHistory;

namespace UteLearningHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<DocumentDto>>> GetDocuments([FromQuery] GetDocumentsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("pending-files-count")]
        [Authorize]
        public async Task<ActionResult<int>> GetPendingFilesCount()
        {
            var result = await _mediator.Send(new GetPendingFilesCountQuery());
            return Ok(result);
        }

        [HttpGet("homepage")]
        public async Task<ActionResult<HomepageDto>> GetHomepage()
        {
            var result = await _mediator.Send(new GetHomepageQuery());
            return Ok(result);
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<PagedResponse<DocumentDto>>> GetMyDocuments([FromQuery] GetMyDocumentsQuery query)
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

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<DocumentDetailDto>> CreateDocument([FromBody] CreateDocumentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<DocumentDetailDto>> UpdateDocument(Guid id, [FromBody] UpdateDocumentCommandRequest request)
        {
            var command = new UpdateDocumentCommand
            {
                Id = id,
                DocumentName = request.DocumentName,
                Description = request.Description,
                SubjectId = request.SubjectId,
                TypeId = request.TypeId,
                TagIds = request.TagIds,
                TagNames = request.TagNames,
                AuthorIds = request.AuthorIds,
                Authors = request.Authors,
                Visibility = request.Visibility,
                FileIdsToRemove = request.FileIdsToRemove,
                CoverFileId = request.CoverFileId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("{id}/files")]
        [Authorize]
        public async Task<ActionResult<DocumentDetailDto>> AddDocumentFile(Guid id, [FromBody] AddDocumentFileCommand command)
        {
            command = command with { DocumentId = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{documentId}/files/{fileId}")]
        [Authorize]
        public async Task<ActionResult<DocumentDetailDto>> UpdateDocumentFile(Guid documentId, Guid fileId, [FromBody] UpdateDocumentFileCommandRequest request)
        {
            var command = new UpdateDocumentFileCommand
            {
                DocumentId = documentId,
                DocumentFileId = fileId,
                Title = request.Title,
                Order = request.Order,
                CoverFileId = request.CoverFileId
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{documentId}/files/{fileId}")]
        [Authorize]
        public async Task<IActionResult> DeleteDocumentFile(Guid documentfileId, Guid fileId)
        {
            var command = new DeleteDocumentFileCommand
            {
                DocumentFileId = fileId
            };

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{documentId}/files/{fileId}/resubmit")]
        [Authorize]
        public async Task<IActionResult> ResubmitDocumentFile(Guid documentId, Guid fileId)
        {
            var command = new ResubmitDocumentFileCommand
            {
                DocumentId = documentId,
                DocumentFileId = fileId
            };

            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSoftDocumentById(Guid id)
        {
            var command = new DeleteDocumentsCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("reading-history")]
        [Authorize]
        public async Task<ActionResult<PagedResponse<ReadingHistoryItemDto>>> GetReadingHistory([FromQuery] GetReadingHistoryQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("files/{fileId}/progress")]
        [Authorize]
        public async Task<ActionResult<DocumentFileProgressDto>> GetDocumentProgress(Guid fileId)
        {
            var query = new GetDocumentProgressQuery { DocumentFileId = fileId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("files/{fileId}/progress")]
        [Authorize]
        public async Task<IActionResult> UpdateDocumentProgress(Guid fileId, [FromBody] UpdateDocumentProgressCommandRequest request)
        {
            var command = new UpdateDocumentProgressCommand
            {
                DocumentFileId = fileId,
                LastPage = request.LastPage
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("files/{fileId}/review")]
        [Authorize]
        public async Task<IActionResult> ReviewDocumentFile(Guid fileId, [FromBody] ReviewDocumentFileCommand command)
        {
            command = command with { DocumentFileId = fileId };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("files/{fileId}/view")]
        public async Task<IActionResult> IncrementViewCount(Guid fileId)
        {
            var command = new IncrementViewCountCommand { DocumentFileId = fileId };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}

