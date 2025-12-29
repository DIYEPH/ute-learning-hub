using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UteLearningHub.Domain.Constaints;
using UteLearningHub.Domain.Entities;

namespace UteLearningHub.Persistence.Configurations;

public class MessageFileConfiguration : IEntityTypeConfiguration<MessageFile>
{
    public void Configure(EntityTypeBuilder<MessageFile> builder)
    {
        builder.ToTable(DbTableNames.MessageFile);

        builder.HasKey(u => new { u.FileId, u.MessageId });

        builder.Property(u => u.FileId).HasColumnName("TepId");
        builder.Property(u => u.MessageId).HasColumnName("TinNhanId");

        builder.HasOne(u => u.Message)
            .WithMany(u => u.MessageFiles)
            .HasForeignKey(u => u.MessageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.File)
            .WithMany(u => u.MessageFiles)
            .HasForeignKey(u => u.FileId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter: exclude when file or message is deleted
        builder.HasQueryFilter(u => !u.File.IsDeleted && !u.Message.IsDeleted);
    }
}
