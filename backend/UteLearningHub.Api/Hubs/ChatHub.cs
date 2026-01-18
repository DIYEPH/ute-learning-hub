using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UteLearningHub.Application.Services.Identity;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Api.Hubs;

[Authorize]
public class ChatHub(
    ICurrentUserService currentUserService,
    IConnectionTracker connectionTracker,
    IUserConversationService userConversationService,
    IMessageHubService messageHubService) : Hub
{
    // Kết nối
    public override async Task OnConnectedAsync()
    {
        if (currentUserService.IsAuthenticated && currentUserService.UserId.HasValue)
            connectionTracker.AddConnection(currentUserService.UserId.Value, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    // Ngắt kết nối
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (currentUserService.UserId.HasValue)
        {
            var userId = currentUserService.UserId.Value;
            connectionTracker.RemoveConnection(Context.ConnectionId);

            // Broadcast offline
            if (!connectionTracker.IsUserOnline(userId))
            {
                var conversationIds = await userConversationService.GetUserConversationIdsAsync(userId);
                foreach (var id in conversationIds)
                    await messageHubService.BroadcastUserOfflineAsync(userId, id);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Tham gia cuộc trò chuyện
    public async Task JoinConversation(Guid conversationId)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        var userId = currentUserService.UserId.Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

        if (connectionTracker.IsUserOnline(userId))
            await messageHubService.BroadcastUserOnlineAsync(userId, conversationId);
    }

    // Rời cuộc trò chuyện
    public async Task LeaveConversation(Guid conversationId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");

    // Typing indicator
    public async Task SendTyping(Guid conversationId, bool isTyping)
    {
        if (!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue)
            return;

        await Clients.GroupExcept($"conversation_{conversationId}", Context.ConnectionId)
            .SendAsync("UserTyping", new
            {
                ConversationId = conversationId,
                UserId = currentUserService.UserId,
                IsTyping = isTyping
            });
    }
}