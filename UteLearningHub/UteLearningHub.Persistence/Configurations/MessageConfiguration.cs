using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;
using UteLearningHub.Persistence.Configurations.Common;
using UteLearningHub.Persistence.Identity;

namespace UteLearningHub.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable(DbTableNames.Message);

        builder.HasKey(t => t.Id);

        builder.Property(u => u.ConversationId).HasColumnName("CuocTroChuyenId");
        builder.Property(u => u.ParentId).HasColumnName("ParentId");
        builder.Property(u => u.Content).HasColumnName("NoiDung");
        builder.Property(u => u.IsEdit).HasColumnName("CoChinhSua");
        builder.Property(u => u.IsPined).HasColumnName("CoDaGhim");

        builder.ApplySoftDelete<Message>()
            .ApplyTrack<Message>()
            .ApplyAudit<Message>();

        builder.HasOne<AppUser>()
            .WithMany(u => u.Messages)
            .HasForeignKey(u => u.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Conversation)
            .WithMany(u => u.Messages)
            .HasForeignKey(u => u.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Message>()
            .WithMany(u => u.Childrens)
            .HasForeignKey(u => u.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
