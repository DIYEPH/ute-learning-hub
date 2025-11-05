using Microsoft.AspNetCore.Identity;
using UteLearningHub.Domain.Constaints.Enums;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Domain.Entities.Base;
using DomainFile = UteLearningHub.Domain.Entities.File;

namespace UteLearningHub.Persistence.Identity;

public class AppUser : IdentityUser<Guid>, ITrackable, ISoftDelete
{
    public Guid MajorId { get; set; }
    public string Introduction { get; set; } = default!;
    public string AvatarUrl { get; set; } = default!;
    public int TrustScrore { get; set; } = 0;
    public bool IsSuggest { get; set; }
    public TrustLever TrustLever { get; set; } = TrustLever.None;
    public Gender Gender { get; set; } = Gender.Other;
    public Major Major { get; set; } = default!;
    public ICollection<UserTrustHistory> UserTrustHistories { get; set; } = [];
    public ICollection<Tag> Tags { get; set; } = [];
    public ICollection<DomainFile> Files { get; set; } = [];
    public ICollection<Notification> CreatedNotifications { get; set; } = [];
    public ICollection<NotificationRecipient> ReceivedNotifications { get; set; } = [];
    public ICollection<Conversation> Conversations { get; set; } = [];
    public ICollection<ConversationMember> ConversationMembers { get; set; } = [];
    public ICollection<ConversationJoinRequest> SentJoinRequests { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
    public ICollection<DocumentReview> DocumentReviews { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Comment> ReviewedComments { get; set; } = [];
    public ICollection<Report> Reports { get; set; } = [];
    public ICollection<Report> ReviewedReports { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    public ICollection<Subject> Subjects { get; set; } = [];

    public byte[] RowVersion { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get ; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; }
}
