using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageHubService
{
    Task BroadcastMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task BroadcastUserOnlineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
    Task BroadcastUserOfflineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}

