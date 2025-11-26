using UteLearningHub.Application.Common.Dtos;

namespace UteLearningHub.Application.Common.Events;

public record MessageQueueEvent(string EventType, MessageDto Payload);

public static class MessageQueueEventTypes
{
    public const string MessageCreated = "MessageCreated";
}

