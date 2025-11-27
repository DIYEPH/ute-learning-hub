using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Infrastructure.Services.Message;

namespace UteLearningHub.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ConnectionTrackerService _connectionTrackerService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ChatHub(ICurrentUserService currentUserService, ConnectionTrackerService connectionTrackerService, IHubContext<ChatHub> hubContext)
    {
        _currentUserService = currentUserService;
        _connectionTrackerService = connectionTrackerService;
        _hubContext = hubContext;
    }

public override async Task OnConnectedAsync()
    {
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;
            _connectionTrackerService.AddConnection(userId, Context.ConnectionId);
            
            // Broadcast to all conversations that user is in
            // TODO: Get user's conversations and notify
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;
            _connectionTrackerService.RemoveConnection(Context.ConnectionId);
            
            // Check if user still has other connections
            if (!_connectionTrackerService.IsUserOnline(userId))
            {
                // User is now offline - broadcast to conversations
                // TODO: Notify conversations
            }
        }
        
        await base.OnDisconnectedAsync(exception);
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
}