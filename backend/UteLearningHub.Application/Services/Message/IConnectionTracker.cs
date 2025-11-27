namespace UteLearningHub.Application.Services.Message;

public interface IConnectionTracker
{
    void AddConnection(Guid userId, string connectionId);
    void RemoveConnection(string connectionId);
    bool IsUserOnline(Guid userId);
    IEnumerable<Guid> GetOnlineUsers(IEnumerable<Guid> userIds);
    IEnumerable<string> GetUserConnectionIds(Guid userId);
}
