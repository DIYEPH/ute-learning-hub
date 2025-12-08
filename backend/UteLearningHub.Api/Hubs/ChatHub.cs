using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IConnectionTracker _connectionTrackerService;
    private readonly IUserConversationService _userConversationService;
    private readonly IMessageHubService _messageHubService;

    public ChatHub(
        ICurrentUserService currentUserService,
        IConnectionTracker connectionTrackerService,
        IUserConversationService userConversationService,
        IMessageHubService messageHubService)
    {
        _currentUserService = currentUserService;
        _connectionTrackerService = connectionTrackerService;
        _userConversationService = userConversationService;
        _messageHubService = messageHubService;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUserService.IsAuthenticated && _currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;

            // Add connection - don't broadcast here, wait until user joins a conversation
            _connectionTrackerService.AddConnection(userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUserService.UserId.HasValue)
        {
            var userId = _currentUserService.UserId.Value;

            // Remove connection
            _connectionTrackerService.RemoveConnection(Context.ConnectionId);

            // Check if user still has other connections
            if (!_connectionTrackerService.IsUserOnline(userId))
            {
                // User is now offline - broadcast to all conversations
                var conversationIds = await _userConversationService.GetUserConversationIdsAsync(userId);

                foreach (var conversationId in conversationIds)
                {
                    await _messageHubService.BroadcastUserOfflineAsync(userId, conversationId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Join vào group của conversation
    public async Task JoinConversation(Guid conversationId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        var userId = _currentUserService.UserId.Value;

        // Join group first
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

        // Notify others in conversation that user is online
        // Only broadcast if user is actually online (has active connections)
        if (_connectionTrackerService.IsUserOnline(userId))
        {
            await _messageHubService.BroadcastUserOnlineAsync(userId, conversationId);
        }
    }

    // Rời khỏi group của conversation
    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
    }

    // Gửi typing indicator
    public async Task SendTyping(Guid conversationId, bool isTyping)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
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