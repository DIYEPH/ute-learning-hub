using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageQueueProducer
{
    Task PublishMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task PublishMessageUpdatedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task PublishMessageDeletedAsync(Guid messageId, Guid conversationId, CancellationToken cancellationToken = default);
    Task PublishMessagePinnedAsync(MessageDto message, CancellationToken cancellationToken = default);
    Task PublishMessageUnpinnedAsync(MessageDto message, CancellationToken cancellationToken = default);
}

