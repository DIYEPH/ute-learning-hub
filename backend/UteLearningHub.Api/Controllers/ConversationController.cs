using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;
using UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;
using UteLearningHub.Application.Features.Conversation.Commands.JoinConversation;
using UteLearningHub.Application.Features.Conversation.Commands.LeaveConversation;
using UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;
using UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversationById;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversationRecommendations;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversations;
using UteLearningHub.Application.Features.Conversation.Queries.GetOnlineMembers;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResponse<ConversationDto>>> GetConversations([FromQuery] GetConversationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> GetConversationById(Guid id)
    {
        var query = new GetConversationByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> CreateConversation([FromBody] CreateConversationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationCommand command)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteConversation(Guid id)
    {
        var command = new DeleteConversationCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id}/join")]
    [Authorize]
    public async Task<ActionResult<ConversationDetailDto>> JoinConversation(Guid id)
    {
        var command = new JoinConversationCommand { ConversationId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id}/leave")]
    [Authorize]
    public async Task<IActionResult> LeaveConversation(Guid id)
    {
        var command = new LeaveConversationCommand { ConversationId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("{conversationId}/online-members")]
    [Authorize]
    public async Task<ActionResult<GetOnlineMembersResponse>> GetOnlineMembers(Guid conversationId)
    {
        var query = new GetOnlineMembersQuery(conversationId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/members/{memberId}/role")]
    [Authorize]
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid memberId, [FromBody] UpdateMemberRoleCommand command)
    {
        command = command with { ConversationId = id, MemberId = memberId };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpGet("recommendations")]
    [Authorize]
    public async Task<ActionResult<GetConversationRecommendationsResponse>> GetRecommendations(
        [FromQuery] int? topK,
        [FromQuery] float? minSimilarity)
    {
        var query = new GetConversationRecommendationsQuery
        {
            TopK = topK,
            MinSimilarity = minSimilarity
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
