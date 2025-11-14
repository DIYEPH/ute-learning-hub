using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationJoinRequestConfiguration : IEntityTypeConfiguration<ConversationJoinRequest>
{
    public void Configure(EntityTypeBuilder<ConversationJoinRequest> builder)
    {
        builder.ToTable(DbTableNames.ConversationJoinRequest);

        builder.HasKey(e => e.Id);
        builder.Property(u => u.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");

        builder.ApplySoftDelete<ConversationJoinRequest>()
            .ApplyTrack<ConversationJoinRequest>()
            .ApplyAudit<ConversationJoinRequest>()
            .ApplyReview<ConversationJoinRequest>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.SentJoinRequests)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Conversation)
            .WithMany(u => u.ConversationJoinRequests)
            .HasForeignKey(u => u.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
