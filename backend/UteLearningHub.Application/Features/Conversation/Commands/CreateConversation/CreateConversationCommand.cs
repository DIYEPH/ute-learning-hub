using MediatR;
using UteLearningHub.Application.Common.Dtos;
using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Commands.CreateConversation;

public record CreateConversationCommand : IRequest<ConversationDetailDto>
{
    public string ConversationName { get; init; } = default!;
    public IList<Guid>? TagIds { get; init; }
    public IList<string>? TagNames { get; init; }
    public ConversitionType ConversationType { get; init; }
    public ConversationVisibility Visibility { get; init; } = ConversationVisibility.Public;
    public Guid? SubjectId { get; init; }
    public bool IsSuggestedByAI { get; init; } = false;
    public bool IsAllowMemberPin { get; init; } = true;
    public string? AvatarUrl { get; init; }
}