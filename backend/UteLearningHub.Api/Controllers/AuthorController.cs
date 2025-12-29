using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Author.Commands.CreateAuthor;
using UteLearningHub.Application.Features.Author.Commands.DeleteAuthor;
using UteLearningHub.Application.Features.Author.Commands.UpdateAuthor;
using UteLearningHub.Application.Features.Author.Queries.GetAuthorById;
using UteLearningHub.Application.Features.Author.Queries.GetAuthors;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthorController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuthorDto>>> GetAuthors([FromQuery] GetAuthorsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDetailDto>> GetAuthorById(Guid id)
    {
        var query = new GetAuthorByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuthorDetailDto>> CreateAuthor([FromBody] CreateAuthorCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<AuthorDetailDto>> UpdateAuthor(Guid id, [FromBody] UpdateAuthorCommandRequest request)
    {
        var command = new UpdateAuthorCommand
        {
            Id = id,
            FullName = request.FullName,
            Description = request.Description
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteAuthor(Guid id)
    {
        var command = new DeleteAuthorCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}

