using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public record UpdateConversationRequest
{
    public Guid Id { get; init; }
    public string? ConversationName { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public ConversitionType? ConversationType { get; init; }
    public ConversationStatus? ConversationStatus { get; init; }
    public Guid? SubjectId { get; init; }
    public bool? IsAllowMemberPin { get; init; }
}
