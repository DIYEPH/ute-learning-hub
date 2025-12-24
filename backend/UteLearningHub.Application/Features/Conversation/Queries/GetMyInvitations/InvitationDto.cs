using UteLearningHub.Domain.Constaints.Enums;

namespace UteLearningHub.Application.Features.Conversation.Queries.GetMyInvitations;

public record InvitationDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public string ConversationName { get; init; } = default!;
    public string? ConversationAvatarUrl { get; init; }
    public int MemberCount { get; init; }
    public Guid InvitedById { get; init; }
    public string InvitedByName { get; init; } = default!;
    public string? InvitedByAvatarUrl { get; init; }
    public string? Message { get; init; }
    public ContentStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
