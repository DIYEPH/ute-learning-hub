using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationJoinRequest : SoftDeletableEntity<Guid>, IAuditable, IReviewable
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = default!;
    public Conversation Conversation { get; set; } = default!;

    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public Guid? ReviewedById { get; set; }
    public string? ReviewNote { get; set; } = default!;
    public DateTimeOffset? ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
}
