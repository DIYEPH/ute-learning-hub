using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Application.Services.Message;

namespace UteLearningHub.Infrastructure.Services.Message;

public class NoOpMessageProducer : IMessageQueueProducer
{
    public Task PublishMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishMessageUpdatedAsync(MessageDto message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishMessageDeletedAsync(Guid messageId, Guid conversationId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishMessagePinnedAsync(MessageDto message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PublishMessageUnpinnedAsync(MessageDto message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
