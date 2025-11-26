using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UteLearningHub.Application.Services.Identity;

namespace UteLearningHub.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public ChatHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    // Join vào group của conversation
    public async Task JoinConversation(Guid conversationId)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    // Rời khỏi group của conversation
    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    // Gửi typing indicator
    public async Task SendTyping(Guid conversationId, bool isTyping)
    {
        if (!_currentUserService.IsAuthenticated)
            return;

        await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
            .SendAsync("UserTyping", new
            {
                ConversationId = conversationId,
                UserId = _currentUserService.UserId,
                IsTyping = isTyping
            });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Cleanup khi user disconnect
        await base.OnDisconnectedAsync(exception);
    }
}