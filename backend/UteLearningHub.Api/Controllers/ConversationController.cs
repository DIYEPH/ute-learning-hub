using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;
using UteLearningHub.Application.Features.Conversation.Commands.DeleteConversation;
using UteLearningHub.Application.Features.Conversation.Commands.GetOrCreateDM;
using UteLearningHub.Application.Features.Conversation.Commands.JoinConversation;
using UteLearningHub.Application.Features.Conversation.Commands.LeaveConversation;
using UteLearningHub.Application.Features.Conversation.Commands.RespondToInvitation;
using UteLearningHub.Application.Features.Conversation.Commands.SendInvitation;
using UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;
using UteLearningHub.Application.Features.Conversation.Commands.UpdateMemberRole;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversationById;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversationRecommendations;
using UteLearningHub.Application.Features.Conversation.Queries.GetConversations;
using UteLearningHub.Application.Features.Conversation.Queries.GetMyInvitations;
using UteLearningHub.Application.Features.Conversation.Queries.GetOnlineMembers;
using UteLearningHub.Application.Features.Conversation.Queries.GetSuggestedUsers;

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
    public async Task<ActionResult<ConversationDetailDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationCommandRequest request)
    {
        var command = new UpdateConversationCommand
        {
            Id = id,
            ConversationName = request.ConversationName,
            TagIds = request.TagIds,
            TagNames = request.TagNames,
            Visibility = request.Visibility,
            ConversationStatus = request.ConversationStatus,
            SubjectId = request.SubjectId,
            IsAllowMemberPin = request.IsAllowMemberPin,
            AvatarUrl = request.AvatarUrl
        };
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
    public async Task<IActionResult> UpdateMemberRole(Guid id, Guid memberId, [FromBody] UpdateMemberRoleCommandRequest request)
    {
        var command = new UpdateMemberRoleCommand
        {
            ConversationId = id,
            MemberId = memberId,
            RoleType = request.RoleType
        };
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
    [HttpGet("{id}/suggested-users")]
    [Authorize]
    public async Task<ActionResult<GetSuggestedUsersResponse>> GetSuggestedUsers(
        Guid id,
        [FromQuery] int? topK,
        [FromQuery] float? minScore)
    {
        var query = new GetSuggestedUsersQuery
        {
            ConversationId = id,
            TopK = topK ?? 10,
            MinScore = minScore ?? 0.3f
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("my-invitations")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<InvitationDto>>> GetMyInvitations([FromQuery] GetMyInvitationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/invitations")]
    [Authorize]
    public async Task<ActionResult<SendInvitationResponse>> SendInvitation(
        Guid id,
        [FromBody] SendInvitationRequest request)
    {
        var command = new SendInvitationCommand
        {
            ConversationId = id,
            InvitedUserId = request.UserId,
            Message = request.Message
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("invitations/{invitationId}/respond")]
    [Authorize]
    public async Task<IActionResult> RespondToInvitation(
        Guid invitationId,
        [FromBody] RespondToInvitationRequest request)
    {
        var command = new RespondToInvitationCommand
        {
            InvitationId = invitationId,
            Accept = request.Accept,
            Note = request.Note
        };
        await _mediator.Send(command);
        return NoContent();
    }

    // ====== DIRECT MESSAGE ENDPOINT ======

    /// <summary>Tìm DM hiện có hoặc tạo mới với tin nhắn đầu tiên</summary>
    [HttpPost("dm/{userId}")]
    [Authorize]
    public async Task<ActionResult<GetOrCreateDMResponse>> GetOrCreateDM(
        Guid userId,
        [FromBody] StartDMRequest? request = null)
    {
        var command = new GetOrCreateDMCommand
        {
            TargetUserId = userId,
            FirstMessage = request?.Message
        };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record SendInvitationRequest(Guid UserId, string? Message);
public record RespondToInvitationRequest(bool Accept, string? Note);
public record StartDMRequest(string? Message);

