using Microsoft.AspNetCore.SignalR;
using UteLearningHub.Api.Hubs;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Api.Services;

public class SignalRMessageHubService : IMessageHubService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRMessageHubService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{message.ConversationId}")
            .SendAsync("MessageReceived", message, cancellationToken);
    }

    public Task BroadcastMessageUpdatedAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{message.ConversationId}")
            .SendAsync("MessageUpdated", message, cancellationToken);
    }

    public Task BroadcastMessageDeletedAsync(Guid messageId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .SendAsync("MessageDeleted", new { MessageId = messageId, ConversationId = conversationId }, cancellationToken);
    }

    public Task BroadcastMessagePinnedAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{message.ConversationId}")
            .SendAsync("MessagePinned", message, cancellationToken);
    }

    public Task BroadcastMessageUnpinnedAsync(MessageDto message, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{message.ConversationId}")
            .SendAsync("MessageUnpinned", message, cancellationToken);
    }

    public Task BroadcastUserOnlineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .SendAsync("UserOnline", new { UserId = userId, ConversationId = conversationId }, cancellationToken);
    }

    public Task BroadcastUserOfflineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group($"conversation_{conversationId}")
            .SendAsync("UserOffline", new { UserId = userId, ConversationId = conversationId }, cancellationToken);
    }
}

