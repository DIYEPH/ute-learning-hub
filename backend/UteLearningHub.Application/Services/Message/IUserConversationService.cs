namespace UteLearningHub.Application.Services.Message;

public interface IUserConversationService
{
    Task<List<Guid>> GetUserConversationIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}