using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageQueueProducer
{
    Task PublishMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default);
}

