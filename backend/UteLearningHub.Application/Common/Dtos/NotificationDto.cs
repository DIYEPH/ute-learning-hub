using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Common.Dtos;

public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid ObjectId { get; init; }
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;
    public string Link { get; init; } = default!;
    public bool IsGlobal { get; init; }
    public DateTimeOffset ExpiredAt { get; init; }
    public NotificationType NotificationType { get; init; }
    public NotificationPriorityType NotificationPriorityType { get; init; }
    public bool IsRead { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReceivedAt { get; init; }
}

public record UnreadCountDto
{
    public int UnreadCount { get; init; }
}
