using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.UpdateConversation;

public record UpdateConversationCommandRequest
{
    public string? ConversationName { get; init; }
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public ConversationVisibility? Visibility { get; init; }
    public ConversationStatus? ConversationStatus { get; init; }
    public Guid? SubjectId { get; init; }
    public bool? IsAllowMemberPin { get; init; }
    public string? AvatarUrl { get; init; }
}
