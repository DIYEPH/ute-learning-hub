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
}

