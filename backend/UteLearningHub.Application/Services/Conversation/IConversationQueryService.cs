using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Services.Conversation;

public interface IConversationQueryService
{
    Task<ConversationDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
