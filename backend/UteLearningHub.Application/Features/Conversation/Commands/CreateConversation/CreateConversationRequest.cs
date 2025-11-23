using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public record CreateConversationRequest
{
    public string ConversationName { get; init; } = default!;
    public string Topic { get; init; } = default!;
    public ConversitionType ConversationType { get; init; }
    public Guid? SubjectId { get; init; }
    public bool IsSuggestedByAI { get; init; } = false;
    public bool IsAllowMemberPin { get; init; } = true;
}
