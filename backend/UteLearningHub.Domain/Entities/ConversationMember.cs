using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities.Base;

namespace UteLearningHub.Domain.Entities;

public class ConversationMember : SoftDeletableEntity<Guid>
{
    public Guid UserId { get; set; }
    public Guid ConversationId { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public bool IsMuted { get; set; }
    public ConversationMemberRoleType ConversationMemberRoleType { get; set; }
    public Conversation Conversation { get; set; } = default!;
    
    // Proposal fields (cho nhóm đề xuất bởi AI)
    public MemberInviteStatus InviteStatus { get; set; } = MemberInviteStatus.Joined;
    public DateTimeOffset? RespondedAt { get; set; }    // Thời điểm phản hồi
    public float? SimilarityScore { get; set; }         // Độ tương đồng với nhóm (0-1)
}
