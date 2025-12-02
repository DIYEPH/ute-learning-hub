using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Services.Message;

public interface IConversationSystemMessageService
{
    Task<MessageDto> CreateAsync(
        Guid conversationId,
        Guid actorId,
        MessageType type,
        Guid? parentId = null,
        CancellationToken cancellationToken = default);
}

