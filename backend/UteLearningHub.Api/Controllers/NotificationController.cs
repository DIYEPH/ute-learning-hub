using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Features.Notification.Commands.CreateNotification;
using UteLearningHub.Application.Features.Notification.Commands.MarkAllAsRead;
using UteLearningHub.Application.Features.Notification.Commands.MarkAsRead;
using UteLearningHub.Application.Features.Notification.Queries.GetNotifications;
using UteLearningHub.Application.Features.Notification.Queries.GetUnreadCount;

namespace UteLearningHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResponse<NotificationDto>>> GetNotifications([FromQuery] GetNotificationsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    [Authorize]
    public async Task<ActionResult<UnreadCountDto>> GetUnreadCount()
    {
        var query = new GetUnreadCountQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{id}/mark-as-read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkAsReadCommand { NotificationId = id };
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("mark-all-as-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var command = new MarkAllAsReadCommand();
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
