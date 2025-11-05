using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Identity;
using DomainFile = UteLearningHub.Domain.Entities.File;
using DomainType = UteLearningHub.Domain.Entities.Type;

namespace UteLearningHub.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public virtual DbSet<Comment> Comments { get; set; }
    public virtual DbSet<Conversation> Conversations { get; set; }
    public virtual DbSet<ConversationJoinRequest> ConversationsJoinRequests { get; set; }
    public virtual DbSet<ConversationMember> ConversationMembers { get; set; }
    public virtual DbSet<Document> Documents { get; set; }
    public virtual DbSet<DocumentFile> DocumentFiles { get; set; }
    public virtual DbSet<DocumentReview> DocumentReviews { get; set; }
    public virtual DbSet<DocumentTag> DocumentTags { get; set; }
    //public virtual DbSet<Event> Events { get; set; }
    public virtual DbSet<Faculty> Faculty { get; set; }
    public virtual DbSet<DomainFile> Files { get; set; }
    public virtual DbSet<Major> Majors { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<MessageFile> MessagesFile { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<NotificationRecipient> NotificationRecipients { get; set; }
    public virtual DbSet<ProfileVector> ProfileVectors { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<DomainType> Types { get; set; }
    public virtual DbSet<UserTrustHistory> UserTrustHistories { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
