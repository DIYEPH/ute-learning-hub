using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageHubService
{
    Task BroadcastMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task BroadcastMessageUpdatedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task BroadcastMessageDeletedAsync(Guid messageId, Guid conversationId, CancellationToken cancellationToken = default);
    Task BroadcastMessagePinnedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task BroadcastMessageUnpinnedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task BroadcastUserOnlineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
    Task BroadcastUserOfflineAsync(Guid userId, Guid conversationId, CancellationToken cancellationToken = default);
}

