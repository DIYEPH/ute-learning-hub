// backend/UteLearningHub.Infrastructure/Services/Message/ConnectionTrackerService.cs

using System.Collections.Concurrent;

namespace UteLearningHub.Infrastructure.Services.Message;

public class ConnectionTrackerService
{
    // UserId -> List of ConnectionIds (1 user có thể có nhiều connections: nhiều tab/device)
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();
    
    // ConnectionId -> UserId
    private readonly ConcurrentDictionary<string, Guid> _connectionUsers = new();

    public void AddConnection(Guid userId, string connectionId)
    {
        _userConnections.AddOrUpdate(
            userId,
            new HashSet<string> { connectionId },
            (key, existing) =>
            {
                existing.Add(connectionId);
                return existing;
            });
        
        _connectionUsers[connectionId] = userId;
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connectionUsers.TryRemove(connectionId, out var userId))
        {
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string>(),
                (key, existing) =>
                {
                    existing.Remove(connectionId);
                    return existing.Count == 0 ? new HashSet<string>() : existing;
                });
        }
    }

    public bool IsUserOnline(Guid userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) 
            && connections.Count > 0;
    }

    public IEnumerable<Guid> GetOnlineUsers(IEnumerable<Guid> userIds)
    {
        return userIds.Where(userId => IsUserOnline(userId));
    }

    public IEnumerable<string> GetUserConnectionIds(Guid userId)
    {
        return _userConnections.TryGetValue(userId, out var connections) 
            ? connections 
            : Enumerable.Empty<string>();
    }
}