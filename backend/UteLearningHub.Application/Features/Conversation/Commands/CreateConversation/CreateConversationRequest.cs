using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public record CreateConversationRequest
{
    public string ConversationName { get; init; } = default!;
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public ConversitionType ConversationType { get; init; }
    public Guid? SubjectId { get; init; }
    public bool IsSuggestedByAI { get; init; } = false;
    public bool IsAllowMemberPin { get; init; } = true;
}
