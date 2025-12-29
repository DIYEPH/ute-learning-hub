using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationInvitation : SoftDeletableEntity<Guid>, IAuditable
{
    public Guid ConversationId { get; set; }
    public Guid InvitedUserId { get; set; }
    public string? Message { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.PendingReview;
    public DateTimeOffset? RespondedAt { get; set; }
    public string? ResponseNote { get; set; }

    // Navigation
    public Conversation Conversation { get; set; } = default!;

    // Auditable
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}

