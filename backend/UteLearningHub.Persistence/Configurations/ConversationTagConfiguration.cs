using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class ConversationTagConfiguration : IEntityTypeConfiguration<ConversationTag>
{
    public void Configure(EntityTypeBuilder<ConversationTag> builder)
    {
        builder.ToTable(DbTableNames.ConversationTag);

        builder.HasKey(u => new { u.TagId, u.ConversationId });

        builder.Property(u => u.TagId).HasColumnName("TheId");
        builder.Property(u => u.ConversationId).HasColumnName("CuocTroChuyenId");

        builder.HasOne(u => u.Tag)
            .WithMany()
            .HasForeignKey(u => u.TagId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Conversation)
            .WithMany(u => u.ConversationTags)
            .HasForeignKey(u => u.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter: exclude tags of deleted conversations
        builder.HasQueryFilter(u => !u.Conversation.IsDeleted);
    }
}

