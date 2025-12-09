using MediatR;

namespace UteLearningHub.Application.Events;

public class UserActivityEvent : INotification
{
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = default!;
    public Dictionary<string, object>? Metadata { get; set; }
}
