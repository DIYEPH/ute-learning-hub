using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Common.Events;

public record MessageQueueEvent(string EventType, MessageDto Payload);

public static class MessageQueueEventTypes
{
    public const string MessageCreated = "MessageCreated";
    public const string MessageUpdated = "MessageUpdated";
    public const string MessageDeleted = "MessageDeleted";
    public const string MessagePinned = "MessagePinned";
    public const string MessageUnpinned = "MessageUnpinned";
}

