using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Message;

public interface IMessageHubService
{
    Task BroadcastMessageCreatedAsync(MessageDto message, CancellationToken cancellationToken = default);
}

