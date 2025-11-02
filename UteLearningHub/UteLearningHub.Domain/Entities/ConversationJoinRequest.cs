using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationJoinRequest : BaseEntity<Guid>, IAuditable<Guid>, IReviewable<Guid>
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = default!;
    public Conversation Conversation { get; set; } = default!;

    public Guid CreatedBy { get; set; }
    public Guid UpdatedBy { get; set; }

    public Guid ReviewedBy { get; set; }
    public string ReviewNote { get; set; } = default!;
    public DateTimeOffset ReviewedAt { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
}
