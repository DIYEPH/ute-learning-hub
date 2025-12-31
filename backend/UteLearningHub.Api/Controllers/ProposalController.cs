using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Features.Conversation.Commands.RespondToProposal;
using UteLearningHub.Application.Features.Conversation.Queries.GetMyProposals;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProposalController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProposalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lấy danh sách proposals của user hiện tại
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<GetMyProposalsResponse>> GetMyProposals()
    {
        var result = await _mediator.Send(new GetMyProposalsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Phản hồi proposal (accept/decline)
    /// </summary>
    [HttpPost("{conversationId}/respond")]
    public async Task<ActionResult<RespondToProposalResponse>> Respond(
        Guid conversationId,
        [FromBody] RespondToProposalRequest request)
    {
        var command = new RespondToProposalCommand
        {
            ConversationId = conversationId,
            Accept = request.Accept
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record RespondToProposalRequest
{
    public bool Accept { get; init; }
}

